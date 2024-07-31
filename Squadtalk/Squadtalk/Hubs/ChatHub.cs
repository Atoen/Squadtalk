using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;
using Squadtalk.Repositories;
using Squadtalk.Services;

namespace Squadtalk.Hubs;

[Authorize]
public partial class ChatHub : Hub<IChatClient>
{
    private readonly ChatConnectionManager _connectionManager;
    private readonly ILogger<ChatHub> _logger;
    private readonly VoiceCallManager _voiceCallManager;
    private readonly UserRepository _userRepository;
    private readonly MessageRepository _messageRepository;
    private readonly ChannelRepository _channelRepository;
    private readonly LiveKitService _liveKitService;

    public ChatHub(
        ChatConnectionManager connectionManager,
        VoiceCallManager voiceCallManager,
        UserRepository userRepository,
        MessageRepository messageRepository,
        ChannelRepository channelRepository,
        LiveKitService liveKitService,
        ILogger<ChatHub> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
        _voiceCallManager = voiceCallManager;
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _channelRepository = channelRepository;
        _liveKitService = liveKitService;
    }

    private IVoiceChatClient VoiceClient(string connectionId) => Clients.Client(connectionId);
    private IVoiceChatClient VoiceGroup(string groupName) => Clients.Group(groupName);
    private IVoiceChatClient OthersInVoiceGroup(string groupName) => Clients.OthersInGroup(groupName);
    private IVoiceChatClient VoiceCaller => Clients.Caller;
    
    private ITextChatClient TextGroup(string groupName) => Clients.Group(groupName);
    private ITextChatClient TextClient(string connectionId) => Clients.Client(connectionId);
    private ITextChatClient TextCaller => Clients.Caller;

    private async Task<ApplicationUser?> GetChannelParticipantAsync(ChannelId channelId, ChannelsInclusionOption channelsInclusionOption = ChannelsInclusionOption.Include)
    {
        var user = await _userRepository.GetUserAsync(Context.User, channelsInclusionOption);
        if (user is null || !user.ParticipatesInChannel(channelId))
        {
            return null;
        }

        return user;
    }
    
    private async Task AddUserToPrivateChannelsAsync(UserDto user, List<Channel> channels, bool isUniqueUserConnection)
    {
        foreach (var channel in channels)
        {
            if (isUniqueUserConnection)
            {
                await TextGroup(channel.Id).UserConnected(user);
            }
            
            await Groups.AddToGroupAsync(Context.ConnectionId, channel.Id);
        }
    }

    public async Task<bool> ChangeGroupName(string? newName, ChannelId channelId, SystemMessageService systemMessageService)
    {
        var userId = Context.User!.GetUserId();
        var channel = await _channelRepository.GetChannelAsync(channelId);
        if (channel is null || !channel.UserParticipatesInChannel(userId))
        {
            return false;
        }

        var user = channel.Participants.SingleOrDefault(x => x.Id == userId);
        if (user is null)
        {
            return false;
        }

        channel.Name = newName;
        var updated = await _channelRepository.UpdateChannelAsync(channel);

        if (!updated)
        {
            return false;
        }

        await TextGroup(channelId).ChannelNameChanged(channelId, newName);

        if (newName is null)
        {
            await systemMessageService.SendChannelNameClearedMessageAsync(user, channelId);
        }
        else
        {
            await systemMessageService.SendChannelNameChangedMessageAsync(user, channelId, newName);
        }

        return true;
    }

    public async Task SendMessage(string messageContent, ChannelId channelId)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            return;
        }

        var addedMessage = await _messageRepository.AddMessageAsync(participant, messageContent, channelId, cancellationToken: Context.ConnectionAborted);
        if (addedMessage is not null)
        {
            await TextGroup(channelId).ReceiveMessage(addedMessage.ToDto());
        }
    }

    public override async Task OnConnectedAsync()
    {
        var user = await _userRepository.GetUserAsync(Context.User, ChannelsInclusionOption.IncludeWithParticipants);
        if (user is null)
        {
            return;
        }

        var dto = user.ToDto();
        var isUniqueConnection = await _connectionManager.Add(user, Context.ConnectionId);
        if (isUniqueConnection)
        {
            await TextGroup(GroupChatModel.GlobalChatId).UserConnected(dto);
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupChatModel.GlobalChatId);
        
        var channelDtos = user.Channels.Select(x => x.ToDto()).ToList();
        
        await TextCaller.GetChannels(channelDtos);
        await TextCaller.GetConnectedUsers(_connectionManager.ConnectedUsers.Select(x => x.ToDto()).ToList());

        if (user.Channels is not { Count: > 0 })
        {
            return;
        }

        await AddUserToPrivateChannelsAsync(dto, user.Channels, isUniqueConnection);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await _userRepository.GetUserAsync(Context.User, ChannelsInclusionOption.Include);
        if (user is null)
        {
            return;
        }

        var dto = user.ToDto();
        
        var allConnectionsClosed = await _connectionManager.Remove(user, Context.ConnectionId);
        if (!allConnectionsClosed)
        {
            return;
        }

        await TextGroup(GroupChatModel.GlobalChatId).UserDisconnected(dto);
        if (user.Channels is not { Count: > 0 })
        {
            return;
        }

        foreach (var channel in user.Channels)
        {
            await TextGroup(channel.Id).UserDisconnected(dto);
        }
    }
}
