using Microsoft.AspNetCore.Components;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Services.SignalR;

namespace Squadtalk.Client.Services;

internal class ChatGroupManager : IChatGroupManager
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly SignalrService _signalrService;
    private readonly IContactManager _contactManager;
    private readonly ILogger<ChatGroupManager> _logger;
    private readonly NavigationManager _navigationManager;

    private readonly Dictionary<GroupId, ChatModel> _allChannels = [];
    private readonly List<GroupChatModel> _groupChats = [];
    private readonly List<DirectMessageModel> _directMessageChannels = [];

    public IReadOnlyCollection<ChatModel> Channels => _allChannels.Values;

    public IEnumerable<GroupChatModel> GroupChats => _groupChats;
    public IEnumerable<DirectMessageModel> DirectMessageChannels => _directMessageChannels;

    public event Action? ChannelsListChanged;

    public event Action? ChannelChanged;
    public event Func<Task>? ChannelChangedAsync;

    public Func<IGroupParticipant, GroupParticipantModel> GroupParticipantProvider { get; }
    public GroupChatModel GlobalGroup { get; } = GroupChatModel.CreateGlobalChat();
    public ChatModel? CurrentChannel { get; private set; }

    private bool _startedScanningStaleTypingState;

    public ChatGroupManager(
        IUserAuthenticationService userAuthenticationService,
        SignalrService signalrService,
        IContactManager contactManager,
        ILogger<ChatGroupManager> logger,
        NavigationManager navigationManager)
    {
        _userAuthenticationService = userAuthenticationService;
        _signalrService = signalrService;
        _contactManager = contactManager;
        _navigationManager = navigationManager;
        _logger = logger;

        GroupParticipantProvider = GetOrCreateGroupParticipantModel;

        _signalrService.ChannelsReceived += ChannelsReceived;
        _signalrService.AddedToChannel += AddedToChannel;
        _signalrService.ChannelNameChanged += OnChannelNameChanged;
        _signalrService.UserIsTyping += UserIsTyping;
        _signalrService.UserStoppedTyping += UserStoppedTyping;
        _signalrService.ChannelParticipantsChanged += ParticipantsChanged;
    }

    #region Public Methods

    public ChatModel? GetChannel(GroupId groupId)
    {
        if (groupId == GlobalGroup.Id)
        {
            return GlobalGroup;
        }

        _allChannels.TryGetValue(groupId, out var channel);
        return channel;
    }

    public ChatModel GetRequiredChannel(GroupId groupId)
    {
        return groupId == GlobalGroup.Id ? GlobalGroup : _allChannels[groupId];
    }

    public async Task OpenChannelAsync(ChatModel chatModel, bool navigate = true)
    {
        if (CurrentChannel == chatModel) return;

        await ChangeChannelAsync(chatModel);

        if (navigate)
        {
            _navigationManager.NavigateTo($"Chats/{CurrentChannel?.Id}");
        }
    }

    public Task ClearChannelSelectionAsync() => ChangeChannelAsync(null);

    public async Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel model)
    {
        if (model.Id == _userAuthenticationService.UserId) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.User.Id == model.Id);
        if (openDirectMessageChannelWithUser is not null)
        {
            await OpenChannelAsync(openDirectMessageChannelWithUser);
            return;
        }

        await OpenChannelAsync(DirectMessageModel.CreateTempChannel(model));
    }

    public async Task UpgradeToPersistentChannelAsync(ChatModel chatModel)
    {
        if (chatModel is not DirectMessageModel dm)
        {
            _logger.LogError("Only direct message channels can be temporary");
            return;
        }

        var channelId = await CreateNewChannelAsync(dm.Other.User);
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

    public async Task<GroupId?> CreateNewChannelAsync(params IEnumerable<UserModel> others)
    {
        var participantsId = others
            .Select(x => x.Id)
            .Append(_userAuthenticationService.UserId);

        var result = await _signalrService.CreateChannelAsync(participantsId);
        return result.Value;
    }

    public async Task CreateAndOpenNewChannelAsync(params IEnumerable<UserModel> others)
    {
        var channelId = await CreateNewChannelAsync(others);
        if (channelId is null) return;

        if (!_allChannels.TryGetValue(channelId, out var createdChannel))
        {
            return;
        }

        await OpenChannelAsync(createdChannel);
    }

    public async Task AddFriendsToGroupAsync(GroupId groupId, params IEnumerable<UserModel> friends)
    {
        var friendsId = friends.Select(x => x.Id);
        var result = await _signalrService.AddFriendsToGroupAsync(groupId, friendsId);

        if (result.ErrorOrValueIs(false))
        {

        }
    }

    #endregion

    #region Event Handlers

    private async Task AddedToChannel(IChatGroup group)
    {
        await AddChannel(group, false);

        ChannelsListChanged?.Invoke();
    }

    private async Task ChannelsReceived(IEnumerable<IChatGroup> channels)
    {
        foreach (var channel in channels)
        {
            await AddChannel(channel, true);
        }

        ChannelsListChanged?.Invoke();
    }

    private void ParticipantsChanged(IChatGroup updatedGroup)
    {
        _logger.LogInformation("Channel {Id} state changed", updatedGroup.Id);
        if (!_allChannels.TryGetValue(updatedGroup.Id, out var channel))
        {
            AddChannel(updatedGroup, false);
            ChannelsListChanged?.Invoke();

            return;
        }

        var others = updatedGroup.Participants
            .Where(x => x.Id != _userAuthenticationService.UserId)
            .Select(GroupParticipantProvider);

        channel.UpdateParticipants(others);
    }

    private void OnChannelNameChanged(GroupId groupId, string? channelName)
    {
        if (!_allChannels.TryGetValue(groupId, out var channel))
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

    private void UserIsTyping(GroupId groupId, UserId userId)
    {
        UpdateTypingState(groupId, userId, isTyping: true);
    }

    private void UserStoppedTyping(GroupId groupId, UserId userId)
    {
        UpdateTypingState(groupId, userId, isTyping: false);
    }

    #endregion

    private GroupParticipantModel GetOrCreateGroupParticipantModel(IGroupParticipant groupParticipant)
    {
        return GroupParticipantModel.Create(groupParticipant, _contactManager.UserModelProvider);
    }

    private Task ChangeChannelAsync(ChatModel? channel)
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

    private Task AddChannel(IChatGroup group, bool bulk)
    {
        if (_allChannels.ContainsKey(group.Id))
        {
            return Task.CompletedTask;
        }

        var model = ChatModel.Create(group, GroupParticipantProvider);
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

        var directMessageChannel = (DirectMessageModel) model;
        _directMessageChannels.Add(directMessageChannel);

        return UpgradeFakeChanelIfNeeded(directMessageChannel);
    }

    private Task UpgradeFakeChanelIfNeeded(DirectMessageModel openedDirectMessageModel)
    {
        if (CurrentChannel is DirectMessageModel dm && dm.IsTemporary() &&
            dm.Other.User.Id == openedDirectMessageModel.Other.User.Id)
        {
            return OpenChannelAsync(openedDirectMessageModel);
        }

        return Task.CompletedTask;
    }

    private void UpdateTypingState(GroupId groupId, UserId userId, bool isTyping)
    {
        _logger.LogInformation("User {UserId} {Action} typing on channel: {ChannelId}",
            userId, isTyping ? "is now" : "stopped", groupId);

        if (!_allChannels.TryGetValue(groupId, out var channel))
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
