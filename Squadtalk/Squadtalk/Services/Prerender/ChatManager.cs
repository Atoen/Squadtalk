using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ChatManager : LazyModelCreator, IChatManager
{
    private static readonly Dictionary<GroupId, ChatModel> EmptyChannels = [];

    private readonly IContactManager _contactManager;

    private Dictionary<GroupId, ChatModel>? _channels;

    private Dictionary<GroupId, ChatModel> LazyChannels => TryCreateModels(ref _channels, EmptyChannels);

    public IReadOnlyCollection<ChatModel> Chats => LazyChannels.Values;

    public Func<IGroupParticipant, GroupParticipantModel> GroupParticipantProvider { get; }

    public ChatModel GlobalChat { get; }

    public ChatModel? CurrentChat { get; private set; }

    event Action? IChatManager.ChatListChanged { add { } remove { } }
    event Action? IChatManager.ChatChanged { add { } remove { } }

    event Func<Task>? IChatManager.ChatChangedAsync { add { } remove { } }

    public ChatManager(
        PrerenderPersistantState prerenderPersistantState,
        IContactManager contactManager) : base(prerenderPersistantState)
    {
        _contactManager = contactManager;

        GlobalChat = ChatModel.CreateGlobalChat(_contactManager.LocalUserModel);
        GroupParticipantProvider = GetOrCreateGroupParticipantModel;
    }

    protected override void CreateModels(PrerenderPersistantState prerenderPersistantState)
    {
        if (prerenderPersistantState.Channels is not { Count: > 0 } channels)
        {
            _channels = EmptyChannels;
            return;
        }

        var channelModels = channels
            .Select(x => ChatModel.Create(x, GroupParticipantProvider))
            .ToDictionary(x => x.Id, x => x);

        _channels = channelModels;
    }

    private GroupParticipantModel GetOrCreateGroupParticipantModel(IGroupParticipant groupParticipant)
    {
        return GroupParticipantModel.Create(groupParticipant, _contactManager.UserModelProvider);
    }

    public Task OpenChannelAsync(ChatModel chatModel, bool navigate = true)
    {
        CurrentChat = chatModel;
        return Task.CompletedTask;
    }

    public ChatModel? GetChannel(GroupId groupId) => LazyChannels.GetValueOrDefault(groupId);

    public ChatModel GetRequiredChannel(GroupId groupId) => LazyChannels[groupId];

    public Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel otherUser) => Task.CompletedTask;

    public Task UpgradeToPersistentChannelAsync(ChatModel chatModel) => Task.CompletedTask;

    public Task<GroupId?> CreateNewChannelAsync(params IEnumerable<UserModel> others) => Task.FromResult<GroupId?>(null);

    public Task CreateAndOpenNewChannelAsync(params IEnumerable<UserModel> others) => Task.CompletedTask;

    public Task AddFriendsToGroupAsync(ChatModel chat, params IEnumerable<UserModel> friends) => Task.CompletedTask;

    public Task ClearChannelSelectionAsync() => Task.CompletedTask;

    public Task MarkMessageSeenAsync(ChatModel chat, MessageModel message) => Task.CompletedTask;

    public Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName) => Task.FromResult(false);

    public Task<bool> DeleteGroupAsync(GroupChatModel groupChat) => Task.FromResult(false);

    public Task<bool> LeaveGroupAsync(GroupChatModel groupChat) => Task.FromResult(false);

    public Task<bool> KickUserAsync(GroupChatModel groupChat, GroupParticipantModel groupParticipant) => Task.FromResult(false);

    public Task<bool> ChangeUserRoleAsync(GroupChatModel groupChat, GroupParticipantModel groupParticipant, GroupRole newRole) => Task.FromResult(false);
}
