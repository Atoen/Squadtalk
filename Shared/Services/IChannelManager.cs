
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface IChannelManager
{
    GroupChatModel GlobalChat { get; }

    ChannelModel? CurrentChannel { get; }

    ChannelState? CurrentChannelState => CurrentChannel?.State;

    IReadOnlyCollection<ChannelModel> Channels { get; }

    IReadOnlyCollection<UserModel> Users { get; }

    event Action<GroupChatModel>? ChannelNameChanged;
    event Action? ChannelsListChanged;
    event Action? ChannelChanged;
    event Func<Task>? ChannelChangedAsync;
    event Action? ConnectedUsersChanged;

    UserModel GetOrCreateUserModel(IChatUser chatUser);

    ChannelModel? GetChannel(ChannelId channelId);

    ChannelModel GetRequiredChannel(ChannelId channelId);

    Task OpenOrCreateTemporaryDirectMessageChannel(UserModel model);
    
    Task UpgradeToPersistentChannelAsync(ChannelModel channelModel);

    Task OpenChannelAsync(ChannelModel channelModel, bool navigate = true);

    Task<ChannelId?> CreateNewChannel(params IEnumerable<UserModel> others);

    Task ClearChannelSelectionAsync();

    Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName);
}
