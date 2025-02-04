using Microsoft.AspNetCore.Components;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;
using Shared.Routing;
using Shared.Services;
using Squadtalk.Client.Services.SignalR;

namespace Squadtalk.Client.Services;

internal class ChatManager : IChatManager
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly SignalrService _signalrService;
    private readonly IContactManager _contactManager;
    private readonly ILogger<ChatManager> _logger;
    private readonly NavigationManager _navigationManager;

    private readonly Dictionary<GroupId, ChatModel> _allChats = [];
    private readonly List<DirectMessageModel> _dms = [];

    public IReadOnlyCollection<ChatModel> Chats => _allChats.Values;

    public event Action? ChatListChanged;
    public event Action? ChatChanged;
    public event Func<Task>? ChatChangedAsync;

    public Func<IGroupParticipant, GroupParticipantModel> GroupParticipantProvider { get; }
    public ChatModel GlobalChat { get; }
    public ChatModel? CurrentChat { get; private set; }

    private bool _startedScanningStaleTypingState;

    public ChatManager(
        IUserAuthenticationService userAuthenticationService,
        SignalrService signalrService,
        IContactManager contactManager,
        ILogger<ChatManager> logger,
        NavigationManager navigationManager)
    {
        _userAuthenticationService = userAuthenticationService;
        _signalrService = signalrService;
        _contactManager = contactManager;
        _navigationManager = navigationManager;
        _logger = logger;

        GlobalChat = ChatModel.CreateGlobalChat(_contactManager.LocalUserModel);
        GroupParticipantProvider = GetOrCreateGroupParticipantModel;

        _signalrService.ChannelsReceived += ChannelsReceived;
        _signalrService.AddedToGroup += AddedToGroup;
        _signalrService.ChannelNameChanged += OnChannelNameChanged;
        _signalrService.UserIsTyping += UserIsTyping;
        _signalrService.UserStoppedTyping += UserStoppedTyping;
        _signalrService.GroupParticipantsChanged += ParticipantsChanged;
        _signalrService.ParticipantRoleChanged += ParticipantRoleChanged;
        _signalrService.GroupDeleted += GroupDeleted;
    }

    #region Public Methods

    public ChatModel? GetChannel(GroupId groupId)
    {
        if (groupId == GlobalChat.Id)
        {
            return GlobalChat;
        }

        _allChats.TryGetValue(groupId, out var channel);
        return channel;
    }

    public ChatModel GetRequiredChannel(GroupId groupId)
    {
        return groupId == GlobalChat.Id ? GlobalChat : _allChats[groupId];
    }

    public async Task OpenChannelAsync(ChatModel chatModel, bool navigate = true)
    {
        if (CurrentChat == chatModel) return;

        await ChangeChannelAsync(chatModel);

        if (navigate)
        {
            _navigationManager.NavigateTo($"Chats/{CurrentChat?.Id}");
        }
    }

    public Task ClearChannelSelectionAsync() => ChangeChannelAsync(null);

    public async Task MarkMessageSeenAsync(ChatModel chat, MessageModel message)
    {
        await _signalrService.MarkMessageSeenAsync(chat.Id, message.Id);
    }

    public async Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel otherUser)
    {
        if (otherUser.Id == _userAuthenticationService.UserId) return;

        var openDirectMessageChannelWithUser = _dms.FirstOrDefault(x => x.Other.Id == otherUser.Id);
        if (openDirectMessageChannelWithUser is not null)
        {
            await OpenChannelAsync(openDirectMessageChannelWithUser);
            return;
        }

        await OpenChannelAsync(DirectMessageModel.CreateTempChannel(_contactManager.LocalUserModel, otherUser));
    }

    public async Task UpgradeToPersistentChannelAsync(ChatModel chatModel)
    {
        if (chatModel is not DirectMessageModel dm)
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
        var result = await _signalrService.ChangeGroupNameAsync(groupChat.Id, newName);
        return result.SuccessAndValueIs(HubResult.Ok);
    }

    public async Task<bool> DeleteGroupAsync(GroupChatModel groupChat)
    {
        var result = await _signalrService.DeleteGroupAsync(groupChat.Id);
        return result.SuccessAndValueIs(HubResult.Ok);
    }

    public async Task<bool> KickUserAsync(GroupChatModel groupChat, GroupParticipantModel groupParticipant)
    {
        var result = await _signalrService.KickUserAsync(groupChat.Id, groupParticipant.Id());
        return result.SuccessAndValueIs(HubResult.Ok);
    }

    public async Task<bool> ChangeUserRoleAsync(GroupChatModel groupChat, GroupParticipantModel groupParticipant, GroupRole newRole)
    {
        var result = await _signalrService.ChangeUserRoleAsync(groupChat.Id, groupParticipant.Id(), newRole);
        return result.SuccessAndValueIs(HubResult.Ok);
    }

    public async Task<bool> LeaveGroupAsync(GroupChatModel groupChat)
    {
        var result = await _signalrService.LeaveGroupAsync(groupChat.Id);
        if (result.ErrorOrValueIsNot(HubResult.Ok))
        {
            return false;
        }

        _allChats.Remove(groupChat.Id);

        if (CurrentChat == groupChat)
        {
            _navigationManager.NavigateTo(Routes.Pages.Chats);
        }

        ChatListChanged?.Invoke();

        return true;
    }

    public async Task<GroupId?> CreateNewChannelAsync(params IEnumerable<UserModel> others)
    {
        var participantsId = others
            .Select(x => x.Id)
            .Append(_userAuthenticationService.UserId);

        var result = await _signalrService.CreateGroupAsync(participantsId);
        return result.Value;
    }

    public async Task CreateAndOpenNewChannelAsync(params IEnumerable<UserModel> others)
    {
        var channelId = await CreateNewChannelAsync(others);
        if (channelId is null) return;

        if (!_allChats.TryGetValue(channelId, out var createdChannel))
        {
            return;
        }

        await OpenChannelAsync(createdChannel);
    }

    public async Task AddFriendsToGroupAsync(ChatModel chat, params IEnumerable<UserModel> friends)
    {
        var friendsId = friends.Select(x => x.Id);
        var result = await _signalrService.AddFriendsToGroupAsync(chat.Id, friendsId);

        if (result.ErrorOrValueIsNot(HubResult.Ok))
        {

        }
    }

    #endregion

    #region Event Handlers

    private async Task AddedToGroup(IChatGroup group)
    {
        await AddChannel(group, false);

        ChatListChanged?.Invoke();
    }

    private async Task ChannelsReceived(IEnumerable<IChatGroup> channels)
    {
        foreach (var channel in channels)
        {
            await AddChannel(channel, true);
        }

        ChatListChanged?.Invoke();
    }

    private void ParticipantsChanged(IChatGroup updatedGroup)
    {
        _logger.LogInformation("Channel {Id} state changed", updatedGroup.Id);
        if (!_allChats.TryGetValue(updatedGroup.Id, out var channel))
        {
            AddChannel(updatedGroup, false);
            ChatListChanged?.Invoke();

            return;
        }

        var others = updatedGroup.Participants.Select(GroupParticipantProvider);
        channel.UpdateParticipants(others);
    }

    private void OnChannelNameChanged(GroupId groupId, string? channelName)
    {
        if (!_allChats.TryGetValue(groupId, out var channel))
        {
            _logger.LogInformation("Non-existent channel name changed");
            return;
        }

        if (channel is not GroupChatModel groupChat)
        {
            return;
        }

        groupChat.CustomName = channelName;

        ChatListChanged?.Invoke();
    }

    private void UserIsTyping(GroupId groupId, UserId userId)
    {
        UpdateTypingState(groupId, userId, isTyping: true);
    }

    private void UserStoppedTyping(GroupId groupId, UserId userId)
    {
        UpdateTypingState(groupId, userId, isTyping: false);
    }

    private void ParticipantRoleChanged(GroupId groupId, UserId userId, GroupRole role)
    {
        if (_allChats.TryGetValue(groupId, out var group))
        {
            group.UpdateParticipantRole(userId, role);
        }
    }

    private void GroupDeleted(GroupId groupId)
    {
        _allChats.Remove(groupId);

        // The current chat is deleted - navigate away
        if (CurrentChat?.Id == groupId)
        {
            CurrentChat = null;
            _navigationManager.NavigateTo(Routes.Pages.Chats);
        }

        ChatListChanged?.Invoke();
    }

    #endregion

    private GroupParticipantModel GetOrCreateGroupParticipantModel(IGroupParticipant groupParticipant)
    {
        return GroupParticipantModel.Create(groupParticipant, _contactManager.UserModelProvider);
    }

    private async Task ChangeChannelAsync(ChatModel? chat)
    {
        if (CurrentChat == chat)
        {
            return;
        }

        CurrentChat = chat;

        if (chat is not null)
        {
            if (chat.LastMessage is { } lastMessage)
            {
                await _signalrService.MarkMessageSeenAsync(chat.Id, lastMessage.Id);
            }

            chat.State.UnreadMessages = 0;
        }

        ChatChanged?.Invoke();
        await ChatChangedAsync.TryInvoke();
    }

    private Task AddChannel(IChatGroup group, bool bulk)
    {
        if (_allChats.ContainsKey(group.Id))
        {
            return Task.CompletedTask;
        }

        var model = ChatModel.Create(group, GroupParticipantProvider);
        // if (!bulk)
        // {
        //     model.State.ScrolledToBeginning = true;
        // }

        _allChats.Add(model.Id, model);

        if (model is not DirectMessageModel dm)
        {
            return Task.CompletedTask;
        }

        _dms.Add(dm);
        return UpgradeFakeChanelIfNeeded(dm);
    }

    private Task UpgradeFakeChanelIfNeeded(DirectMessageModel openedDirectMessageModel)
    {
        if (CurrentChat is DirectMessageModel dm && dm.IsTemporary() &&
            dm.Other.Id == openedDirectMessageModel.Other.Id)
        {
            return OpenChannelAsync(openedDirectMessageModel);
        }

        return Task.CompletedTask;
    }

    private void UpdateTypingState(GroupId groupId, UserId userId, bool isTyping)
    {
        _logger.LogInformation("User {UserId} {Action} typing on channel: {ChannelId}",
            userId, isTyping ? "is now" : "stopped", groupId);

        if (!_allChats.TryGetValue(groupId, out var channel))
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
            foreach (var channel in Chats)
            {
                channel.State.RemoveStaleTyping(now);
            }
        }
    }
}
