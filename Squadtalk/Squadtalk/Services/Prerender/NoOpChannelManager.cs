using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class NoOpChannelManager : IChannelManager
{
    private static readonly GroupChatModel GlobalChatModel = GroupChatModel.CreateGlobalChat();
    private static readonly Dictionary<ChannelId, ChannelModel> EmptyChannels = [];
    private static readonly Dictionary<UserId, UserModel> EmptyUsers = [];

    private readonly PrerenderPersistantState _prerenderPersistantState;
    private readonly IUserAuthenticationService _authenticationService;

    public GroupChatModel GlobalChat => GlobalChatModel;
    public ChannelModel? CurrentChannel => null;

    public IReadOnlyCollection<UserModel> Users => LazyUsers.Values;
    public IReadOnlyCollection<ChannelModel> Channels => LazyChannels.Values;

    event Action<GroupChatModel>? IChannelManager.ChannelNameChanged { add { } remove { } }
    event Action? IChannelManager.ChannelsListChanged { add { } remove { } }
    event Action? IChannelManager.ChannelChanged { add { } remove { } }
    event Func<Task>? IChannelManager.ChannelChangedAsync { add { } remove { } }
    event Action? IChannelManager.ConnectedUsersChanged { add { } remove { } }

    public NoOpChannelManager(
        PrerenderPersistantState prerenderPersistantState,
        IUserAuthenticationService authenticationService)
    {
        _prerenderPersistantState = prerenderPersistantState;
        _authenticationService = authenticationService;
    }

    private bool _modelsCreated;

    private Dictionary<UserId, UserModel>? _users;
    private Dictionary<ChannelId, ChannelModel>? _channels;

    private Dictionary<UserId, UserModel> LazyUsers => GetUserModels();
    private Dictionary<ChannelId, ChannelModel> LazyChannels => GetChannelModels();

    private Dictionary<UserId, UserModel> GetUserModels()
    {
        if (!_prerenderPersistantState.ContainsData)
        {
            return _modelsCreated
                ? _users ?? EmptyUsers
                : EmptyUsers;
        }

        if (!_modelsCreated)
        {
            CreateModels();
        }

        return _users ?? EmptyUsers;
    }

    private Dictionary<ChannelId, ChannelModel> GetChannelModels()
    {
        if (!_prerenderPersistantState.ContainsData)
        {
            return _modelsCreated
                ? _channels ?? EmptyChannels
                : EmptyChannels;
        }

        if (!_modelsCreated)
        {
            CreateModels();
        }

        return _channels ?? EmptyChannels;
    }

    private void CreateModels()
    {
        var onlineUsers = _prerenderPersistantState.OnlineUsers ?? [];
        var channels = _prerenderPersistantState.Channels ?? [];

        var currentUserId = _authenticationService.UserId;

        var userModels = onlineUsers
            .Select(x => UserModel.Create(x, UserStatus.Online))
            .ToDictionary(x => x.Id, x => x);

        var channelModels = channels
            .Select(x => ChannelModel.Create(x, currentUserId, UserFactory))
            .ToDictionary(x => x.Id, x => x);

        _modelsCreated = true;

        _users = userModels;
        _channels = channelModels;

        return;

        UserModel UserFactory(IChatUser user)
        {
            if (userModels.TryGetValue(user.Id, out var model))
            {
                return model;
            }

            var newModel = UserModel.Create(user, UserStatus.Unknown);
            userModels[user.Id] = newModel;

            return newModel;
        }
    }

    public ChannelModel? GetChannel(ChannelId channelId) => LazyChannels.GetValueOrDefault(channelId);

    public ChannelModel GetRequiredChannel(ChannelId channelId) => throw new InvalidOperationException();

    public Task OpenOrCreateTemporaryDirectMessageChannel(UserModel model) => Task.CompletedTask;

    public Task CreateRealDirectMessageChannel(ChannelModel channelModel) => Task.CompletedTask;

    public Task OpenChannelAsync(ChannelModel channelModel, bool navigate = true) => Task.CompletedTask;

    public Task<ChannelId?> CreateNewChannel(IEnumerable<UserModel> others) => Task.FromResult<ChannelId?>(null);

    public Task ClearChannelSelectionAsync() => Task.CompletedTask;

    public Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName) => Task.FromResult(false);
}
