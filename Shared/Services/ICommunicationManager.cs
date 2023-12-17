using Shared.Communication;
using Shared.Models;

namespace Shared.Services;

public interface ICommunicationManager
{
    TextChannel? GetChannel(string channelId);
    
    TextChannel CurrentChannel { get; }

    TextChannelState CurrentChannelState => CurrentChannel.State;
    
    IReadOnlyList<GroupChat> GroupChats { get; }

    IReadOnlyList<DirectMessageChannel> DirectMessageChannels { get; }
    
    IReadOnlyList<UserModel> Users { get; }

    event Action ChannelChanged;

    event Action StateChanged;

    event Func<Task> StateChangedAsync; 
    
    void ChangeChannel(string globalChatId);
    
    Task OpenOrCreateFakeDirectMessageChannel(UserModel model);
    
    Task CreateRealDirectMessageChannel(TextChannel channel);
}