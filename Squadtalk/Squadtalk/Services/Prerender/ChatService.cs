using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ChatService : IChatService
{
    public GroupChatModel GlobalChat { get; }
    public ChannelModel? CurrentChannel { get; }
    public IEnumerable<GroupChatModel> GroupChats { get; }
    public IEnumerable<DirectMessageChannelModel> DirectMessageChannels { get; }
    public IEnumerable<ChannelModel> AllChannels { get; }
    public IEnumerable<UserModel> Users { get; }
    public event Action<GroupChatModel>? ChannelNameChanged;
    public event Action? ChannelsListChanged;
    public event Action? ChannelChanged;
    public event Func<Task>? ChannelChangedAsync;
    public event Action? ConnectedUsersChanged;
    public ChannelModel? GetChannel(ChannelId channelId) => throw new NotImplementedException();
    public ChannelModel GetRequiredChannel(ChannelId channelId) => throw new NotImplementedException();
    public Task OpenOrCreateFakeDirectMessageChannel(UserModel model) => throw new NotImplementedException();
    public Task CreateRealDirectMessageChannel(ChannelModel channelModel) => throw new NotImplementedException();
    public Task OpenChannelAsync(ChannelModel channelModel, bool navigate = true) => throw new NotImplementedException();
    public Task<ChannelId?> CreateNewChannel(IEnumerable<UserModel> others) => throw new NotImplementedException();
    public Task ClearChannelSelectionAsync() => throw new NotImplementedException();
    public Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName) => throw new NotImplementedException();
}
