using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using RestSharp;
using Shared.Communication;
using Shared.Data;
using Shared.DTOs;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public class TextChatService : ITextChatService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly RestClient _restClient;
    private readonly ISignalrService _signalrService;
    private readonly ILogger<TextChatService> _logger;
    private readonly NavigationManager _navigationManager;

    private readonly List<TextChannel> _allChannels = [];
    private readonly List<GroupChat> _groupChats = [];
    private readonly List<DirectMessageChannel> _directMessageChannels = [];

    public IReadOnlyList<UserModel> Users => UserModel.Models;
    public IReadOnlyList<TextChannel> AllChannels => _allChannels;
    public IReadOnlyList<GroupChat> GroupChats => _groupChats;
    public IReadOnlyList<DirectMessageChannel> DirectMessageChannels => _directMessageChannels;

    private UserId? _userId;

    public TextChatService(AuthenticationStateProvider authenticationStateProvider, RestClient restClient,
        ISignalrService signalrService, ILogger<TextChatService> logger, NavigationManager navigationManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _restClient = restClient;
        _signalrService = signalrService;
        _navigationManager = navigationManager;
        _logger = logger;

        _signalrService.UserConnected += userDto => UserConnected(userDto, false);
        _signalrService.UserDisconnected += UserDisconnected;
        _signalrService.ConnectedUsersReceived += ReceivedConnectedUsers;
        _signalrService.TextChannelsReceived += TextChannelsReceived;
        _signalrService.AddedToTextChannel += AddedToTextChannel;
    }

    public event Action? StateChanged;
    public event Func<Task>? StateChangedAsync;
    public event Action? ChannelChanged;
    public event Func<Task>? ChannelChangedAsync;

    public TextChannel? CurrentChannel { get; private set; }

    public TextChannel? GetChannel(ChannelId id)
    {
        return id == GroupChat.GlobalChatId
            ? GroupChat.GlobalChat
            : AllChannels.FirstOrDefault(x => x.Id == id);
    }

    public async Task OpenChannelAsync(TextChannel channel)
    {
        await ChangeChannelAsync(channel);
        _navigationManager.NavigateTo("Messages/Chat");
    }

    public Task ClearChannelSelectionAsync() => ChangeChannelAsync(null);

    public async Task OpenOrCreateFakeDirectMessageChannel(UserModel model)
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var id = authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier);

        if (id == model.Id) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == model.Id);
        if (openDirectMessageChannelWithUser is not null)
        {
            await OpenChannelAsync(openDirectMessageChannelWithUser);
            return;
        }

        await OpenChannelAsync(DirectMessageChannel.CreateFakeChannel(model));
    }

    public async Task CreateRealDirectMessageChannel(TextChannel channel)
    {
        _userId ??= await GetUserIdAsync();

        var otherUserId = ((DirectMessageChannel) channel).Other.Id;
        var participants = new List<UserId> { _userId, otherUserId };

        var channelId = await OpenNewChannel(participants);

        if (channelId is not null && GetChannel(channelId) is { } openedChannel)
        {
            await ChangeChannelAsync(openedChannel);
        }
    }

    private Task ChangeChannelAsync(TextChannel? channel)
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

    private async Task<ChannelId?> OpenNewChannel(List<UserId> participants)
    {
        var request = new RestRequest("api/message/createChannel")
            .AddBody(participants);

        try
        {
            var createdChannelId = await _restClient.PostAsync<ChannelId>(request);
            return createdChannelId;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while sending create channel request");
            return null;
        }
    }

    private async Task AddedToTextChannel(ChannelDto dto)
    {
        await AddChannel(dto, false);
        await StateChangedAsync.TryInvoke();
    }

    private async Task TextChannelsReceived(IEnumerable<ChannelDto> dtos)
    {
        foreach (var channelDto in dtos)
        {
            await AddChannel(channelDto, true);
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async Task AddChannel(ChannelDto channelDto, bool bulk)
    {
        if (_allChannels.Exists(x => x.Id == channelDto.Id)) return;
        _userId ??= await GetUserIdAsync();

        var model = CreateChannelModel(channelDto, _userId);
        if (!bulk)
        {
            model.State.ReachedEnd = true;
        }

        _allChannels.Add(model);

        if (model is GroupChat groupChat)
        {
            _groupChats.Add(groupChat);
        }
        else
        {
            var directMessageChannel = (DirectMessageChannel) model;
            _directMessageChannels.Add(directMessageChannel);

            await CheckIfNeedToUpgradeCurrentFakeChannelToReal(directMessageChannel);
        }

        if (!bulk)
        {
            StateChanged?.Invoke();
            await StateChangedAsync.TryInvoke();
        }
    }

    private Task CheckIfNeedToUpgradeCurrentFakeChannelToReal(DirectMessageChannel openedDirectMessageChannel)
    {
        if (CurrentChannel is DirectMessageChannel { Id.Value: DirectMessageChannel.FakeChannelIdValue } currentFakeDm &&
            currentFakeDm.Other.Id == openedDirectMessageChannel.Other.Id)
        {
            return ChangeChannelAsync(openedDirectMessageChannel);
        }

        return Task.CompletedTask;
    }

    private TextChannel CreateChannelModel(ChannelDto channelDto, UserId id)
    {
        var lastMessageIsByCurrentUser = channelDto.LastMessage?.Author.Id == id;

        _logger.LogInformation("Creating model");

        var othersInChannel = channelDto.Participants.Where(x => x.Id != id).ToList();

        TextChannel channel = othersInChannel switch
        {
            [var other] => new DirectMessageChannel(UserModel.GetOrCreate(other), channelDto.Id),
            { Count: > 1 } => new GroupChat(othersInChannel.Select(UserModel.GetOrCreate), channelDto.Id),
            _ => throw new InvalidOperationException()
        };

        return channel.WithLastMessage(channelDto.LastMessage, lastMessageIsByCurrentUser);
    }

    private async Task ReceivedConnectedUsers(IEnumerable<UserDto> users)
    {
        foreach (var user in users)
        {
            _logger.LogInformation("User {@User} connected in bulk", user.Username);
            await UserConnected(user, true);
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async Task UserConnected(UserDto userDto, bool bulkAdd)
    {
        _userId ??= await GetUserIdAsync();

        if (userDto.Id == _userId) return;

        var model = UserModel.GetOrCreate(userDto);
        model.Status = UserStatus.Online;

        if (!bulkAdd)
        {
            StateChanged?.Invoke();
            await StateChangedAsync.TryInvoke();
        }
    }

    private async Task UserDisconnected(UserDto userDto)
    {
        _userId ??= await GetUserIdAsync();

        if (userDto.Id == _userId) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == userDto.Id);
        if (openDirectMessageChannelWithUser is null)
        {
            UserModel.Models.RemoveAll(x => x.Id == userDto.Id);
        }
        else
        {
            UserModel.Models.First(x => x.Id == userDto.Id).Status = UserStatus.Offline;
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async ValueTask<UserId> GetUserIdAsync()
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var claimValue = authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier);
        return new UserId(claimValue);
    }
}