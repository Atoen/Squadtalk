using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface IChatGroupManager
{
    event Action? ChannelsListChanged;
    event Action? ChannelChanged;
    event Func<Task>? ChannelChangedAsync;

    Func<IGroupParticipant, GroupParticipantModel> GroupParticipantProvider { get; }

    ChatModel GlobalChat { get; }

    ChatModel? CurrentChannel { get; }

    ChannelState? CurrentChannelState => CurrentChannel?.State;

    IReadOnlyCollection<ChatModel> Channels { get; }

    ChatModel? GetChannel(GroupId groupId);

    ChatModel GetRequiredChannel(GroupId groupId);

    Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel model);

    Task UpgradeToPersistentChannelAsync(ChatModel chatModel);

    Task OpenChannelAsync(ChatModel chatModel, bool navigate = true);

    Task<GroupId?> CreateNewChannelAsync(params IEnumerable<UserModel> others);

    Task CreateAndOpenNewChannelAsync(params IEnumerable<UserModel> others);

    Task AddFriendsToGroupAsync(GroupId groupId, params IEnumerable<UserModel> friends);

    Task ClearChannelSelectionAsync();

    Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName);
}
