using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Models;
using Shared.Results;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ChannelManager(
    PrerenderPersistantState prerenderPersistantState,
    IUserAuthenticationService authenticationService) : IChannelManager, IContactManager
{
    private static readonly GroupChatModel GlobalChatModel = GroupChatModel.CreateGlobalChat();
    private static readonly Dictionary<ChannelId, ChannelModel> EmptyChannels = [];
    private static readonly Dictionary<UserId, UserModel> EmptyFriends = [];

    private bool _modelsCreated;

    private Dictionary<UserId, UserModel>? _friends;
    private Dictionary<ChannelId, ChannelModel>? _channels;

    private Dictionary<UserId, UserModel> LazyFriends => TryCreateModels(ref _friends, EmptyFriends);
    private Dictionary<ChannelId, ChannelModel> LazyChannels => TryCreateModels(ref _channels, EmptyChannels);

    public IReadOnlyCollection<ChannelModel> Channels => LazyChannels.Values;

    public GroupChatModel GlobalChat => GlobalChatModel;
    public ChannelModel? CurrentChannel { get; private set; }

    event Action? IContactManager.FriendListChanged { add { } remove { } }
    event Action? IContactManager.FriendRequestsChanged { add { } remove { } }
    event Action<IncomingFriendRequest>? IContactManager.FriendRequestReceived { add { } remove { } }

    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public IReadOnlyCollection<UserModel> AllContacts { get; } = [];
    public IReadOnlyCollection<UserModel> FriendList => LazyFriends.Values;
    public IReadOnlyCollection<UserModel> OtherContacts => LazyFriends.Values;
    public IReadOnlyCollection<IncomingFriendRequest> IncomingFriendRequests { get; } = [];
    public IReadOnlyCollection<OutgoingFriendRequest> OutgoingFriendRequests { get; } = [];

    event Action<GroupChatModel>? IChannelManager.ChannelNameChanged { add { } remove { } }
    event Action? IChannelManager.ChannelsListChanged { add { } remove { } }
    event Action? IChannelManager.ChannelChanged { add { } remove { } }
    event Func<Task>? IChannelManager.ChannelChangedAsync { add { } remove { } }

    private Dictionary<TKey, TValue> TryCreateModels<TKey, TValue>(ref Dictionary<TKey, TValue>? storage, Dictionary<TKey, TValue> empty)
        where TKey : notnull
    {
        if (!prerenderPersistantState.ContainsData)
        {
            return _modelsCreated ? storage ?? empty : empty;
        }

        if (!_modelsCreated)
        {
            CreateModels();
        }

        return storage ?? empty;
    }

    private void CreateModels()
    {
        var friends = prerenderPersistantState.Friends ?? Array.Empty<UserDto>();
        var channels = prerenderPersistantState.Channels ?? Array.Empty<ChannelDto>();

        var currentUserId = authenticationService.UserId;

        var friendModels = friends
            .Select(x => UserModel.Create(x))
            .ToDictionary(x => x.Id, x => x);

        var channelModels = channels
            .Select(x => ChannelModel.Create(x, currentUserId, user => GetOrCreateUser(user, friendModels)))
            .ToDictionary(x => x.Id, x => x);

        _modelsCreated = true;

        _friends = friendModels;
        _channels = channelModels;
    }

    public UserModel GetOrCreateUserModel(IChatUser chatUser)
    {
        return GetOrCreateUser(chatUser, LazyFriends);
    }

    public Task<FriendRequestResult?> SendFriendRequestAsync(string recipientUsername) => throw new NotImplementedException();
    public Task<CancelFriendRequestResult?> CancelFriendRequest(OutgoingFriendRequest friendRequest) => throw new NotImplementedException();
    public Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted) => throw new NotImplementedException();
    public Task<RemoveFriendResult?> RemoveFriendAsync(UserModel userModel) => throw new NotImplementedException();
    public Task<List<UserModel>> GetFriendsAsync() => Task.FromResult(new List<UserModel>());
    public Task<List<PendingFriendRequestDto>> GetPendingFriendRequestsAsync() => Task.FromResult(new List<PendingFriendRequestDto>());

    public Task OpenChannelAsync(ChannelModel channelModel, bool navigate = true)
    {
        CurrentChannel = channelModel;
        return Task.CompletedTask;
    }

    private static UserModel GetOrCreateUser(IChatUser user, Dictionary<UserId, UserModel> cache)
    {
        if (!cache.TryGetValue(user.Id, out var model))
        {
            model = UserModel.Create(user, UserStatus.Unknown);
            cache[user.Id] = model;
        }

        return model;
    }

    public ChannelModel? GetChannel(ChannelId channelId) => LazyChannels.GetValueOrDefault(channelId);

    public ChannelModel GetRequiredChannel(ChannelId channelId) => LazyChannels[channelId];

    public Task OpenOrCreateTemporaryDirectMessageChannel(UserModel model) => Task.CompletedTask;

    public Task UpgradeToPersistentChannelAsync(ChannelModel channelModel) => Task.CompletedTask;

    public Task<ChannelId?> CreateNewChannel(params IEnumerable<UserModel> others) => Task.FromResult<ChannelId?>(null);

    public Task ClearChannelSelectionAsync() => Task.CompletedTask;

    public Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName) => Task.FromResult(false);
}
