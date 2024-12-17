using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class NoOpChannelManager : IChannelManager
{
    private static readonly GroupChatModel GlobalChatModel = GroupChatModel.CreateGlobalChat();

    private readonly PrerenderPersistantState _prerenderPersistantState;
    private readonly IUserAuthenticationService _authenticationService;

    public GroupChatModel GlobalChat => GlobalChatModel;

    public ChannelModel? CurrentChannel => null;

    private Dictionary<UserId, UserModel>? _users;
    private Dictionary<ChannelId, ChannelModel>? _channels;

    private Dictionary<UserId, UserModel>? LazyUsers => _users ??= CreateUserModels();
    private Dictionary<ChannelId, ChannelModel>? LazyChannels => _channels ??= CreateChannelModels();

    public IReadOnlyCollection<ChannelModel> Channels
    {
        get
        {
            if (LazyChannels is null) return [];
            return LazyChannels.Values;
        }
    }

    public IReadOnlyCollection<UserModel> Users
    {
        get
        {
            if (LazyUsers is null) return [];
            return LazyUsers.Values;
        }
    }

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

    private Dictionary<UserId, UserModel>? CreateUserModels()
    {
        if (_prerenderPersistantState.Users is not { Count: > 0 } users)
        {
            return null;
        }

        var models = new Dictionary<UserId, UserModel>(users.Count);

        foreach (var userDto in users)
        {
            models[userDto.Id] = UserModel.Create(userDto);
        }

        return models;
    }

    private UserModel CreateUserModel(IChatUser user)
    {
        if (LazyUsers?.TryGetValue(user.Id, out var model) == true)
        {
            return model;
        }

        model = UserModel.Create(user);
        if (LazyUsers is not null)
        {
            LazyUsers[user.Id] = model;
        }

        return model;
    }

    private Dictionary<ChannelId, ChannelModel>? CreateChannelModels()
    {
        if (_prerenderPersistantState.Channels is not { Count: > 0 } channels)
        {
            return null;
        }

        var models = new Dictionary<ChannelId, ChannelModel>(channels.Count);
        var userFactory = CreateUserModel;
        var channelModels = channels.Select(x => ChannelModel.Create(x, _authenticationService.UserId, userFactory));
        foreach (var channelModel in channelModels)
        {
            models.Add(channelModel.Id, channelModel);
        }

        return models;
    }

    public ChannelModel? GetChannel(ChannelId channelId) => null;

    public ChannelModel GetRequiredChannel(ChannelId channelId) => throw new InvalidOperationException();

    public Task OpenOrCreateTemporaryDirectMessageChannel(UserModel model) => Task.CompletedTask;

    public Task CreateRealDirectMessageChannel(ChannelModel channelModel) => Task.CompletedTask;

    public Task OpenChannelAsync(ChannelModel channelModel, bool navigate = true) => Task.CompletedTask;

    public Task<ChannelId?> CreateNewChannel(IEnumerable<UserModel> others) => Task.FromResult<ChannelId?>(null);

    public Task ClearChannelSelectionAsync() => Task.CompletedTask;

    public Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName) => Task.FromResult(false);
}
