using Shared.Communication;
using Shared.Data;
using Shared.Models;

namespace Shared.Services;

public interface ITextChatService
{
    TextChannelModel? GetChannel(ChannelId id);
    
    GroupChatModel GlobalChat { get; }
    
    TextChannelModel? CurrentChannel { get; }

    TextChannelState? CurrentChannelState => CurrentChannel?.State;
    
    IReadOnlyList<GroupChatModel> GroupChats { get; }

    IReadOnlyList<DirectMessageChannelModel> DirectMessageChannels { get; }
    
    IReadOnlyList<TextChannelModel> AllChannels { get; }
    
    IReadOnlyList<UserModel> Users { get; }

    event Action? ChannelChanged;

    event Action? StateChanged;

    event Func<Task>? StateChangedAsync;
    
    event Func<Task>? ChannelChangedAsync; 
    
    Task OpenOrCreateFakeDirectMessageChannel(UserModel model);
    
    Task CreateRealDirectMessageChannel(TextChannelModel channelModel);

    Task OpenChannelAsync(TextChannelModel channelModel);

    Task ClearChannelSelectionAsync();
}