using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Communication;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public class ChatService : IChatService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ICreateTextChannelRequestHandler _createTextChannelRequestHandler;
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<ChatService> _logger;
    private readonly NavigationManager _navigationManager;

    private readonly Dictionary<ChannelId, ChannelModel> _allChannels = [];
    private readonly List<GroupChatModel> _groupChats = [];
    private readonly List<DirectMessageChannelModel> _directMessageChannels = [];

    public IEnumerable<UserModel> Users => UserModel.Models;
    public IEnumerable<ChannelModel> AllChannels => _allChannels.Values;
    public IEnumerable<GroupChatModel> GroupChats => _groupChats;
    public IEnumerable<DirectMessageChannelModel> DirectMessageChannels => _directMessageChannels;

    private UserId? _userId;

    public ChatService(
        AuthenticationStateProvider authenticationStateProvider,
        ICreateTextChannelRequestHandler createTextChannelRequestHandler,
        ICommunicationService communicationService,
        ILogger<ChatService> logger,
        NavigationManager navigationManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _createTextChannelRequestHandler = createTextChannelRequestHandler;
        _communicationService = communicationService;
        _navigationManager = navigationManager;
        _logger = logger;

        _communicationService.UserConnected += user => UserConnected(user, false);
        _communicationService.UserDisconnected += UserDisconnected;
        _communicationService.ConnectedUsersReceived += ReceivedConnectedUsers;
        _communicationService.ChannelsReceived += ChannelsReceived;
        _communicationService.AddedToChannel += AddedToChannel;
    }

    public event Action? StateChanged;
    public event Func<Task>? StateChangedAsync;
    public event Action? ChannelChanged;
    public event Func<Task>? ChannelChangedAsync;

    public GroupChatModel GlobalChat { get; } = GroupChatModel.CreateGlobalChat();
    public ChannelModel? CurrentChannel { get; private set; }

    public ChannelModel? GetChannel(ChannelId channelId)
    {
        if (channelId == GlobalChat.Id)
        {
            return GlobalChat;
        }

        _allChannels.TryGetValue(channelId, out var channel);
        return channel;
    }

    public ChannelModel GetRequiredChannel(ChannelId channelId)
    {
        return channelId == GlobalChat.Id ? GlobalChat : _allChannels[channelId];
    }

    public async Task OpenChannelAsync(ChannelModel channelModel)
    {
        await ChangeChannelAsync(channelModel);
        _navigationManager.NavigateTo("Messages/Chat");
    }

    public Task ClearChannelSelectionAsync() => ChangeChannelAsync(null);

    public async Task OpenOrCreateFakeDirectMessageChannel(UserModel model)
    {
        _userId ??= await GetUserIdAsync();

        if (_userId == model.Id) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == model.Id);
        if (openDirectMessageChannelWithUser is not null)
        {
            await OpenChannelAsync(openDirectMessageChannelWithUser);
            return;
        }

        await OpenChannelAsync(DirectMessageChannelModel.CreateFakeChannel(model));
    }

    public async Task CreateRealDirectMessageChannel(ChannelModel channelModel)
    {
        _userId ??= await GetUserIdAsync();

        var otherUserId = ((DirectMessageChannelModel) channelModel).Other.Id;
        var participants = new List<UserId> { (UserId) _userId, otherUserId };

        var channelId = await OpenNewChannel(participants);

        if (channelId != default && GetChannel(channelId) is { } openedChannel)
        {
            await ChangeChannelAsync(openedChannel);
        }
    }

    private Task ChangeChannelAsync(ChannelModel? channel)
    {
        if (CurrentChannel == channel)
        {
            return Task.CompletedTask;
        }

        CurrentChannel = channel;
        if (CurrentChannel is not null)
        {
            CurrentChannel.State.UnreadMessages = 0;
        }

        ChannelChanged?.Invoke();
        return ChannelChangedAsync.TryInvoke();
    }

    private Task<ChannelId?> OpenNewChannel(List<UserId> participants)
    {
        return _createTextChannelRequestHandler.CreateTextChannelAsync(participants);
    }

    private async Task AddedToChannel(IChatChannel channel)
    {
        await AddChannel(channel, false);
        await StateChangedAsync.TryInvoke();
    }

    private async Task ChannelsReceived(IEnumerable<IChatChannel> channels)
    {
        foreach (var channel in channels)
        {
            await AddChannel(channel, true);
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async Task AddChannel(IChatChannel channel, bool bulk)
    {
        if (_allChannels.ContainsKey(channel.Id)) return;
        _userId ??= await GetUserIdAsync();

        var model = CreateChannelModel(channel, (UserId) _userId);
        if (!bulk)
        {
            model.State.ReachedEnd = true;
        }

        _allChannels.Add(model.Id, model);

        if (model is GroupChatModel groupChat)
        {
            _groupChats.Add(groupChat);
        }
        else
        {
            var directMessageChannel = (DirectMessageChannelModel) model;
            _directMessageChannels.Add(directMessageChannel);

            await CheckIfNeedToUpgradeCurrentFakeChannelToReal(directMessageChannel);
        }

        if (!bulk)
        {
            StateChanged?.Invoke();
            await StateChangedAsync.TryInvoke();
        }
    }

    private Task CheckIfNeedToUpgradeCurrentFakeChannelToReal(DirectMessageChannelModel openedDirectMessageChannelModel)
    {
        if (CurrentChannel is DirectMessageChannelModel dm && dm.IsFake() &&
            dm.Other.Id == openedDirectMessageChannelModel.Other.Id)
        {
            return ChangeChannelAsync(openedDirectMessageChannelModel);
        }

        return Task.CompletedTask;
    }

    private ChannelModel CreateChannelModel(IChatChannel channel, UserId id)
    {
        var lastMessageIsByCurrentUser = channel.LastMessage?.Author.Id == id;

        _logger.LogInformation("Creating model");

        var othersInChannel = channel.Participants.Where(x => x.Id != id).ToList();

        ChannelModel model = othersInChannel switch
        {
            [var other] => new DirectMessageChannelModel(UserModel.GetOrCreate(other), channel.Id),
            { Count: > 1 } => new GroupChatModel(othersInChannel.Select(UserModel.GetOrCreate), channel.Id),
            _ => throw new InvalidOperationException()
        };

        return model.WithLastMessage(channel.LastMessage, lastMessageIsByCurrentUser);
    }

    private async Task ReceivedConnectedUsers(IEnumerable<IChatUser> users)
    {
        foreach (var user in users)
        {
            _logger.LogInformation("User {@User} connected in bulk", user.Username);
            await UserConnected(user, true);
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async Task UserConnected(IChatUser user, bool bulkAdd)
    {
        _userId ??= await GetUserIdAsync();

        if (user.Id == _userId) return;

        var model = UserModel.GetOrCreate(user);
        model.Status = UserStatus.Online;

        if (!bulkAdd)
        {
            StateChanged?.Invoke();
            await StateChangedAsync.TryInvoke();
        }
    }

    private async Task UserDisconnected(IChatUser user)
    {
        _userId ??= await GetUserIdAsync();

        if (user.Id == _userId) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == user.Id);
        if (openDirectMessageChannelWithUser is null)
        {
            UserModel.Models.RemoveAll(x => x.Id == user.Id);
        }
        else
        {
            UserModel.Models.First(x => x.Id == user.Id).Status = UserStatus.Offline;
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async ValueTask<UserId> GetUserIdAsync()
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return UserId.Parse(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
    }
}
