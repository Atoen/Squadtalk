using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Models;
using Shared.Reactive;

namespace Shared.Services;

public interface IChatManager
{
    event Action? ChatListChanged;
    event Action? ChatChanged;
    event Func<Task>? ChatChangedAsync;

    Func<IGroupParticipant, GroupParticipantModel> GroupParticipantProvider { get; }

    ChatModel GlobalChat { get; }

    ChatModel? CurrentChat { get; }

    ChannelState? CurrentChannelState => CurrentChat?.State;

    IObservableCollection<ChatModel> Chats { get; }

    ChatModel? GetChannel(GroupId groupId);

    ChatModel GetRequiredChannel(GroupId groupId);

    Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel otherUser);

    Task UpgradeToPersistentChannelAsync(ChatModel chatModel);

    Task OpenChannelAsync(ChatModel chatModel, bool navigate = true, bool replace = false);

    Task<ChatModel?> CreateNewChatAsync(params IEnumerable<UserModel> others);

    Task CreateAndOpenNewChannelAsync(params IEnumerable<UserModel> others);

    Task AddFriendsToGroupAsync(ChatModel chat, params IEnumerable<UserModel> friends);

    void ClearChannelSelection();

    Task MarkMessageSeenAsync(ChatModel chat, MessageModel message);

    Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName);

    Task<bool> DeleteGroupAsync(GroupChatModel groupChat);

    Task<bool> LeaveGroupAsync(GroupChatModel groupChat);

    Task<bool> KickUserAsync(GroupChatModel groupChat, GroupParticipantModel groupParticipant);

    Task<bool> ChangeUserRoleAsync(GroupChatModel groupChat, GroupParticipantModel groupParticipant, GroupRole newRole);
}
