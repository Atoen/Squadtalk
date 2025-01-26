using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface IChannelManager
{
    GroupChatModel GlobalChat { get; }

    ChannelModel? CurrentChannel { get; }

    ChannelState? CurrentChannelState => CurrentChannel?.State;

    IReadOnlyCollection<ChannelModel> Channels { get; }

    event Action? ChannelsListChanged;
    event Action? ChannelChanged;
    event Func<Task>? ChannelChangedAsync;

    ChannelModel? GetChannel(ChannelId channelId);

    ChannelModel GetRequiredChannel(ChannelId channelId);

    Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel model);
    
    Task UpgradeToPersistentChannelAsync(ChannelModel channelModel);

    Task OpenChannelAsync(ChannelModel channelModel, bool navigate = true);

    Task<ChannelId?> CreateNewChannelAsync(params IEnumerable<UserModel> others);

    Task AddFriendsToGroupAsync(ChannelId channelId, params IEnumerable<UserModel> friends);

    Task ClearChannelSelectionAsync();

    Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName);
}
