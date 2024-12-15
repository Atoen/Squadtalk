using Microsoft.AspNetCore.Components;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

internal class ChatService : IChatService
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly CreateTextChannelRequestHandler _createTextChannelRequestHandler;
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<ChatService> _logger;
    private readonly NavigationManager _navigationManager;

    private readonly Func<IChatUser, UserModel> _userModelProvider;
    private readonly Dictionary<UserId, UserModel> _users = [];
    private readonly Dictionary<ChannelId, ChannelModel> _allChannels = [];

    private readonly List<GroupChatModel> _groupChats = [];
    private readonly List<DirectMessageChannelModel> _directMessageChannels = [];

    public IEnumerable<UserModel> Users => _users.Values;
    public IEnumerable<ChannelModel> AllChannels => _allChannels.Values;

    public IEnumerable<GroupChatModel> GroupChats => _groupChats;
    public IEnumerable<DirectMessageChannelModel> DirectMessageChannels => _directMessageChannels;

    public event Action? ChannelsListChanged;
    public event Action<GroupChatModel>? ChannelNameChanged;

    public event Action? ChannelChanged;
    public event Func<Task>? ChannelChangedAsync;

    public event Action? ConnectedUsersChanged;

    public GroupChatModel GlobalChat { get; } = GroupChatModel.CreateGlobalChat();
    public ChannelModel? CurrentChannel { get; private set; }

    public ChatService(
        IUserAuthenticationService userAuthenticationService,
        CreateTextChannelRequestHandler createTextChannelRequestHandler,
        ICommunicationService communicationService,
        ILogger<ChatService> logger,
        NavigationManager navigationManager)
    {
        _userAuthenticationService = userAuthenticationService;
        _createTextChannelRequestHandler = createTextChannelRequestHandler;
        _communicationService = communicationService;
        _navigationManager = navigationManager;
        _logger = logger;

        _userModelProvider = GetUserModel;

        _communicationService.UserConnected += user => UserConnected(user, false, false);
        _communicationService.UserDisconnected += UserDisconnected;
        _communicationService.ConnectedUsersReceived += ReceivedConnectedUsers;
        _communicationService.ChannelsReceived += ChannelsReceived;
        _communicationService.AddedToChannel += AddedToChannel;
        _communicationService.ChannelNameChanged += OnChannelNameChanged;
    }

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

    public async Task OpenChannelAsync(ChannelModel channelModel, bool navigate = true)
    {
        if (CurrentChannel == channelModel) return;

        await ChangeChannelAsync(channelModel);

        if (navigate)
        {
            _navigationManager.NavigateTo($"Channels/{CurrentChannel?.Id}");
        }
    }

    public Task ClearChannelSelectionAsync() => ChangeChannelAsync(null);

    public async Task OpenOrCreateFakeDirectMessageChannel(UserModel model)
    {
        if (model.Id == _userAuthenticationService.UserId) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == model.Id);
        if (openDirectMessageChannelWithUser is not null)
        {
            await OpenChannelAsync(openDirectMessageChannelWithUser);
            return;
        }

        await OpenChannelAsync(DirectMessageChannelModel.CreateTempChannel(model));
    }

    public async Task CreateRealDirectMessageChannel(ChannelModel channelModel)
    {
        var others = channelModel switch
        {
            DirectMessageChannelModel directMessageChannelModel => [directMessageChannelModel.Other],
            GroupChatModel groupChatModel => groupChatModel.Others,
            _ => throw new ArgumentOutOfRangeException(nameof(channelModel))
        };

        var channelId = await CreateNewChannel(others);
        if (channelId is not null && GetChannel(channelId) is { } openedChannel)
        {
            await ChangeChannelAsync(openedChannel);
        }
    }

    public Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName)
    {
        return _communicationService.ChangeChannelNameAsync(newName, groupChat.Id);
    }

    public async Task<ChannelId?> CreateNewChannel(IEnumerable<UserModel> others)
    {
        var participantsId = others.Select(x => x.Id).Append(_userAuthenticationService.UserId);

        return await _createTextChannelRequestHandler.CreateTextChannelAsync(participantsId);
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

    private async Task AddedToChannel(IChatChannel channel)
    {
        await AddChannel(channel, false);

        ChannelsListChanged?.Invoke();
    }

    private async Task ChannelsReceived(IEnumerable<IChatChannel> channels)
    {
        foreach (var channel in channels)
        {
            await AddChannel(channel, true);
        }

        ChannelsListChanged?.Invoke();
    }

    private async Task AddChannel(IChatChannel channel, bool bulk)
    {
        if (_allChannels.ContainsKey(channel.Id)) return;

        var model = ChannelModel.Create(channel, _userAuthenticationService.UserId, _userModelProvider);
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

            await UpgradeFakeChanelIfNeeded(directMessageChannel);
        }
    }

    private Task UpgradeFakeChanelIfNeeded(DirectMessageChannelModel openedDirectMessageChannelModel)
    {
        if (CurrentChannel is DirectMessageChannelModel dm && dm.IsFake() &&
            dm.Other.Id == openedDirectMessageChannelModel.Other.Id)
        {
            return ChangeChannelAsync(openedDirectMessageChannelModel);
        }

        return Task.CompletedTask;
    }

    private Task ReceivedConnectedUsers(IEnumerable<IChatUser> users, bool fromPersistedData)
    {
        foreach (var user in users)
        {
            _logger.LogInformation("User {@User} connected in bulk", user.Username);
            UserConnected(user, true, fromPersistedData);
        }

        ConnectedUsersChanged?.Invoke();

        return Task.CompletedTask;
    }

    private UserModel GetUserModel(IChatUser chatUser)
    {
        if (_users.TryGetValue(chatUser.Id, out var model))
        {
            return model;
        }

        model = UserModel.Create(chatUser);
        _users[chatUser.Id] = model;

        return model;
    }

    private Task UserConnected(IChatUser connectedUser, bool bulkAdd, bool fromPersistedData)
    {
        if (connectedUser.Id == _userAuthenticationService.UserId)
        {
            return Task.CompletedTask;
        }

        var model = GetUserModel(connectedUser);
        model.Status = fromPersistedData ? UserStatus.Unknown : UserStatus.Online;

        if (!bulkAdd)
        {
            ConnectedUsersChanged?.Invoke();
        }

        return Task.CompletedTask;
    }

    private Task UserDisconnected(IChatUser disconnectedUser)
    {
        if (disconnectedUser.Id == _userAuthenticationService.UserId)
        {
            return Task.CompletedTask;
        }

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == disconnectedUser.Id);
        if (openDirectMessageChannelWithUser is null)
        {
            _users.Remove(disconnectedUser.Id);
        }
        else
        {
            _users[disconnectedUser.Id].Status = UserStatus.Offline;
        }

        ConnectedUsersChanged?.Invoke();

        return Task.CompletedTask;
    }

    private Task OnChannelNameChanged(ChannelId channelId, string? channelName)
    {
        if (!_allChannels.TryGetValue(channelId, out var channel))
        {
            _logger.LogInformation("Non-existent channel name changed");
            return Task.CompletedTask;
        }

        if (channel is not GroupChatModel groupChat)
        {
            return Task.CompletedTask;
        }

        groupChat.CustomName = channelName;

        ChannelNameChanged?.Invoke(groupChat);
        ChannelsListChanged?.Invoke();

        return Task.CompletedTask;
    }
}
