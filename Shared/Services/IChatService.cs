
using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface IChatService
{
    GroupChatModel GlobalChat { get; }
    
    ChannelModel? CurrentChannel { get; }

    ChannelState? CurrentChannelState => CurrentChannel?.State;
    
    IEnumerable<GroupChatModel> GroupChats { get; }

    IEnumerable<DirectMessageChannelModel> DirectMessageChannels { get; }

    IEnumerable<ChannelModel> AllChannels { get; }

    IEnumerable<UserModel> Users { get; }

    event Action<GroupChatModel>? ChannelNameChanged;

    event Action? ChannelsListChanged;
    event Func<Task>? ChannelsListChangedAsync;

    event Action? ChannelChanged;
    event Func<Task>? ChannelChangedAsync;

    event Action? ConnectedUsersChanged;

    ChannelModel? GetChannel(ChannelId channelId);

    ChannelModel GetRequiredChannel(ChannelId channelId);

    Task OpenOrCreateFakeDirectMessageChannel(UserModel model);
    
    Task CreateRealDirectMessageChannel(ChannelModel channelModel);

    Task OpenChannelAsync(ChannelModel channelModel);

    Task<ChannelId?> CreateNewChannel(IEnumerable<UserModel> others);

    Task ClearChannelSelectionAsync();

    Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName);
}
