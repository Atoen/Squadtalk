using Shared.Communication;
using Shared.Data;
using Shared.Models;

namespace Shared.Services;

public interface ITextChatService
{
    TextChannel? GetChannel(ChannelId id);
    
    TextChannel? CurrentChannel { get; }

    TextChannelState? CurrentChannelState => CurrentChannel?.State;
    
    IReadOnlyList<GroupChat> GroupChats { get; }

    IReadOnlyList<DirectMessageChannel> DirectMessageChannels { get; }
    
    IReadOnlyList<TextChannel> AllChannels { get; }
    
    IReadOnlyList<UserModel> Users { get; }

    event Action? ChannelChanged;

    event Action? StateChanged;

    event Func<Task>? StateChangedAsync;
    
    event Func<Task>? ChannelChangedAsync; 
    
    Task OpenOrCreateFakeDirectMessageChannel(UserModel model);
    
    Task CreateRealDirectMessageChannel(TextChannel channel);

    Task OpenChannelAsync(TextChannel channel);

    Task ClearChannelSelectionAsync();
}