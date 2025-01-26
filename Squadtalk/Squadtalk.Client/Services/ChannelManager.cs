using Microsoft.AspNetCore.Components;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Services.SignalR;

namespace Squadtalk.Client.Services;

internal class ChannelManager : IChannelManager
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly SignalrService _signalrService;
    private readonly IContactManager _contactManager;
    private readonly ILogger<ChannelManager> _logger;
    private readonly NavigationManager _navigationManager;

    private readonly Dictionary<ChannelId, ChannelModel> _allChannels = [];
    private readonly List<GroupChatModel> _groupChats = [];
    private readonly List<DirectMessageChannelModel> _directMessageChannels = [];

    public IReadOnlyCollection<ChannelModel> Channels => _allChannels.Values;

    public IEnumerable<GroupChatModel> GroupChats => _groupChats;
    public IEnumerable<DirectMessageChannelModel> DirectMessageChannels => _directMessageChannels;

    public event Action? ChannelsListChanged;

    public event Action? ChannelChanged;
    public event Func<Task>? ChannelChangedAsync;

    public GroupChatModel GlobalChat { get; } = GroupChatModel.CreateGlobalChat();
    public ChannelModel? CurrentChannel { get; private set; }

    private bool _startedScanningStaleTypingState;

    public ChannelManager(
        IUserAuthenticationService userAuthenticationService,
        SignalrService signalrService,
        IContactManager contactManager,
        ILogger<ChannelManager> logger,
        NavigationManager navigationManager)
    {
        _userAuthenticationService = userAuthenticationService;
        _signalrService = signalrService;
        _contactManager = contactManager;
        _navigationManager = navigationManager;
        _logger = logger;

        _signalrService.ChannelsReceived += ChannelsReceived;
        _signalrService.AddedToChannel += AddedToChannel;
        _signalrService.ChannelNameChanged += OnChannelNameChanged;
        _signalrService.UserIsTyping += UserIsTyping;
        _signalrService.UserStoppedTyping += UserStoppedTyping;
        _signalrService.ChannelParticipantsChanged += ParticipantsChanged;
    }

    #region Public Methods

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
            _navigationManager.NavigateTo($"Chats/{CurrentChannel?.Id}");
        }
    }

    public Task ClearChannelSelectionAsync() => ChangeChannelAsync(null);

    public async Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel model)
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

    public async Task UpgradeToPersistentChannelAsync(ChannelModel channelModel)
    {
        if (channelModel is not DirectMessageChannelModel dm)
        {
            _logger.LogError("Only direct message channels can be temporary");
            return;
        }

        var channelId = await CreateNewChannelAsync(dm.Other);
        if (channelId is not null && GetChannel(channelId) is { } openedChannel)
        {
            await ChangeChannelAsync(openedChannel);
        }
    }

    public async Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName)
    {
        var result = await _signalrService.ChangeChannelNameAsync(groupChat.Id, newName);
        return result.ValueOr(false);
    }

    public async Task<ChannelId?> CreateNewChannelAsync(params IEnumerable<UserModel> others)
    {
        var participantsId = others
            .Select(x => x.Id)
            .Append(_userAuthenticationService.UserId);

        var result = await _signalrService.CreateChannelAsync(participantsId);
        return result.Value;
    }

    public async Task AddFriendsToGroupAsync(ChannelId channelId, params IEnumerable<UserModel> friends)
    {
        var friendsId = friends.Select(x => x.Id);
        var result = await _signalrService.AddFriendsToGroupAsync(channelId, friendsId);

        if (result.ErrorOrValueIs(false))
        {

        }
    }

    #endregion

    #region Event Handlers

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

    private void ParticipantsChanged(IChatChannel updatedChannel)
    {
        _logger.LogInformation("Channel {Id} state changed", updatedChannel.Id);
        if (!_allChannels.TryGetValue(updatedChannel.Id, out var channel))
        {
            AddChannel(updatedChannel, false);
            ChannelsListChanged?.Invoke();

            return;
        }

        var others = updatedChannel.Participants
            .Where(x => x.Id != _userAuthenticationService.UserId)
            .Select(_contactManager.UserModelProvider);

        channel.UpdateParticipants(others);
    }

    private void OnChannelNameChanged(ChannelId channelId, string? channelName)
    {
        if (!_allChannels.TryGetValue(channelId, out var channel))
        {
            _logger.LogInformation("Non-existent channel name changed");
            return;
        }

        if (channel is not GroupChatModel groupChat)
        {
            return;
        }

        groupChat.CustomName = channelName;

        ChannelsListChanged?.Invoke();
    }

    private void UserIsTyping(ChannelId channelId, UserId userId)
    {
        UpdateTypingState(channelId, userId, isTyping: true);
    }

    private void UserStoppedTyping(ChannelId channelId, UserId userId)
    {
        UpdateTypingState(channelId, userId, isTyping: false);
    }

    #endregion

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

    private Task AddChannel(IChatChannel channel, bool bulk)
    {
        if (_allChannels.ContainsKey(channel.Id))
        {
            return Task.CompletedTask;
        }

        var model = ChannelModel.Create(channel, _userAuthenticationService.UserId, _contactManager.UserModelProvider);
        // if (!bulk)
        // {
        //     model.State.ScrolledToBeginning = true;
        // }

        _allChannels.Add(model.Id, model);

        if (model is GroupChatModel groupChat)
        {
            _groupChats.Add(groupChat);
            return Task.CompletedTask;
        }

        var directMessageChannel = (DirectMessageChannelModel) model;
        _directMessageChannels.Add(directMessageChannel);

        return UpgradeFakeChanelIfNeeded(directMessageChannel);
    }

    private Task UpgradeFakeChanelIfNeeded(DirectMessageChannelModel openedDirectMessageChannelModel)
    {
        if (CurrentChannel is DirectMessageChannelModel dm && dm.IsTemporary() &&
            dm.Other.Id == openedDirectMessageChannelModel.Other.Id)
        {
            return OpenChannelAsync(openedDirectMessageChannelModel);
        }

        return Task.CompletedTask;
    }

    private void UpdateTypingState(ChannelId channelId, UserId userId, bool isTyping)
    {
        _logger.LogInformation("User {UserId} {Action} typing on channel: {ChannelId}",
            userId, isTyping ? "is now" : "stopped", channelId);

        if (!_allChannels.TryGetValue(channelId, out var channel))
        {
            return;
        }

        if (isTyping)
        {
            StartScanning();
        }

        if (isTyping)
        {
            channel.State.UserIsTyping(userId);
        }
        else
        {
            channel.State.UserStoppedTyping(userId);
        }
    }

    private void StartScanning()
    {
        if (Interlocked.Exchange(ref _startedScanningStaleTypingState, true))
        {
            return;
        }

        _logger.LogInformation("Started scanning typing state");

        _ = RemoveStaleTypingStates();
    }

    private async Task RemoveStaleTypingStates()
    {
        using var timer = new PeriodicTimer(TypingTiming.StaleScanInterval);
        while (await timer.WaitForNextTickAsync())
        {
            var now = DateTime.Now;
            foreach (var channel in Channels)
            {
                channel.State.RemoveStaleTyping(now);
            }
        }
    }
}
