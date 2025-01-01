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
    private static readonly Dictionary<UserId, UserModel> EmptyUsers = [];

    private bool _modelsCreated;

    private Dictionary<UserId, UserModel>? _users;
    private Dictionary<ChannelId, ChannelModel>? _channels;

    private Dictionary<UserId, UserModel> LazyUsers => TryCreateModels(ref _users, EmptyUsers);
    private Dictionary<ChannelId, ChannelModel> LazyChannels => TryCreateModels(ref _channels, EmptyChannels);

    public IReadOnlyCollection<ChannelModel> Channels => LazyChannels.Values;

    public GroupChatModel GlobalChat => GlobalChatModel;
    public ChannelModel? CurrentChannel { get; private set; }

    public event Action<IncomingFriendRequest>? FriendRequestReceived;
    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public IReadOnlyCollection<UserModel> AllContacts => LazyUsers.Values;
    public IReadOnlyCollection<UserModel> FriendList { get; } = [];
    public IReadOnlyCollection<UserModel> OtherContacts => LazyUsers.Values;
    public IReadOnlyCollection<IncomingFriendRequest> IncomingFriendRequests { get; }
    public IReadOnlyCollection<OutgoingFriendRequest> OutgoingFriendRequests { get; }

    event Action<GroupChatModel>? IChannelManager.ChannelNameChanged { add { } remove { } }
    event Action? IChannelManager.ChannelsListChanged { add { } remove { } }
    event Action? IChannelManager.ChannelChanged { add { } remove { } }
    event Func<Task>? IChannelManager.ChannelChangedAsync { add { } remove { } }

    event Action? IContactManager.ContactsStateChanged { add { } remove { } }
    event Action<UserModel>? IContactManager.ContactDisconnected { add { } remove { } }
    event Action<UserModel>? IContactManager.ContactConnected { add { } remove { } }

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
        var onlineUsers = prerenderPersistantState.OnlineUsers ?? Array.Empty<UserDto>();
        var channels = prerenderPersistantState.Channels ?? Array.Empty<ChannelDto>();

        var currentUserId = authenticationService.UserId;

        var userModels = onlineUsers
            .Select(x => UserModel.Create(x, UserStatus.Online))
            .ToDictionary(x => x.Id, x => x);

        var channelModels = channels
            .Select(x => ChannelModel.Create(x, currentUserId, user => GetOrCreateUser(user, userModels)))
            .ToDictionary(x => x.Id, x => x);

        _modelsCreated = true;
        _users = userModels;
        _channels = channelModels;
    }

    public UserModel GetOrCreateUserModel(IChatUser chatUser)
    {
        return GetOrCreateUser(chatUser, LazyUsers);
    }

    public Task<FriendRequestResult?> SendFriendRequestAsync(string recipientUsername) => throw new NotImplementedException();
    public Task<CancelFriendRequestResult?> CancelFriendRequest(OutgoingFriendRequest friendRequest) => throw new NotImplementedException();
    public Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted) => throw new NotImplementedException();
    public Task<CancelFriendRequestResult?> CancelFriendRequest(FriendRequestId requestId) => throw new NotImplementedException();
    public Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(FriendRequestId requestId, bool accepted) => throw new NotImplementedException();
    public Task<RemoveFriendResult?> RemoveFriendAsync(UserId friendId) => throw new NotImplementedException();
    public Task<List<UserModel>> GetFriendsAsync() => throw new NotImplementedException();
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
