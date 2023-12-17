using Shared.Communication;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services;

public class ServersideCommunicationManager : ICommunicationManager
{
    public TextChannel? GetChannel(string channelId)
    {
        return null;
    }

    public TextChannel CurrentChannel => GroupChat.GlobalChat;
    public IReadOnlyList<GroupChat> GroupChats { get; } = [];
    public IReadOnlyList<DirectMessageChannel> DirectMessageChannels { get; } = [];
    public IReadOnlyList<UserModel> Users { get; } = [];
    
    public event Action? ChannelChanged;
    public event Action? StateChanged;
    public event Func<Task>? StateChangedAsync;

    public void ChangeChannel(string globalChatId)
    {
        // throw new NotImplementedException();
    }

    public Task OpenOrCreateFakeDirectMessageChannel(UserModel model)
    {
        // throw new NotImplementedException();
        return Task.CompletedTask;
    }

    public Task CreateRealDirectMessageChannel(TextChannel channel)
    {
        // throw new NotImplementedException();
        return Task.CompletedTask;
    }
}