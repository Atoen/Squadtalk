using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Models;
using Shared.Results;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ContactManager : LazyModelCreator, IContactManager
{
    private static readonly Dictionary<UserId, UserModel> EmptyFriends = [];
    private static readonly Dictionary<FriendRequestId, IncomingFriendRequest> EmptyIncomingFriendRequests = [];

    private readonly PrerenderPersistantState _prerenderPersistantState;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly Dictionary<UserId, UserModel> _userModels = [];

    private Dictionary<UserId, UserModel>? _friends;
    private Dictionary<FriendRequestId, IncomingFriendRequest>? _incomingFriendRequests;

    private Dictionary<UserId, UserModel> LazyFriends => TryCreateModels(ref _friends, EmptyFriends);
    private Dictionary<FriendRequestId, IncomingFriendRequest> LazyIncomingFriendRequests => TryCreateModels(ref _incomingFriendRequests, EmptyIncomingFriendRequests);

    public IReadOnlyCollection<UserModel> FriendList => LazyFriends.Values;
    public IReadOnlyCollection<IncomingFriendRequest> IncomingFriendRequests => LazyIncomingFriendRequests.Values;
    public IReadOnlyCollection<OutgoingFriendRequest> OutgoingFriendRequests { get; } = [];

    event Action? IContactManager.FriendListChanged { add { } remove { } }
    event Action? IContactManager.FriendRequestsChanged { add { } remove { } }
    event Action<IncomingFriendRequest>? IContactManager.FriendRequestReceived { add { } remove { } }
    event Action? IContactManager.StatusChanged { add { } remove { } }

    public UserStatus UserStatus => UserStatus.Unknown;

    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public ContactManager(
        PrerenderPersistantState prerenderPersistantState,
        IUserAuthenticationService userAuthenticationService) : base(prerenderPersistantState)
    {
        _prerenderPersistantState = prerenderPersistantState;
        _userAuthenticationService = userAuthenticationService;

        UserModelProvider = GetOrCreateUserModel;
    }

    protected override void CreateModels(PrerenderPersistantState prerenderPersistantState)
    {
        var friends = _prerenderPersistantState.Friends ?? Array.Empty<UserDto>();
        _friends = friends.Select(UserModelProvider)
            .ToDictionary(x => x.Id, x => x);

        var userId = _userAuthenticationService.UserId;
        var requests = _prerenderPersistantState.FriendRequests ?? Array.Empty<PendingFriendRequestDto>();

        var incomingFriendRequests = new Dictionary<FriendRequestId, IncomingFriendRequest>();
        foreach (var x in requests)
        {
            if (x.Recipient.Id != userId) continue;

            // During prerendering only the number of incoming friend requests is displayed
            // No need to create the actual models
            incomingFriendRequests.Add(x.Id, null!);
        }

        _incomingFriendRequests = incomingFriendRequests;
    }

    public UserModel? FindUserById(UserId userId) => _userModels.GetValueOrDefault(userId);

    public UserModel GetOrCreateUserModel(IChatUser chatUser)
    {
        if (!_userModels.TryGetValue(chatUser.Id, out var model))
        {
            model = UserModel.Create(chatUser, _userAuthenticationService.UserId);
            _userModels[chatUser.Id] = model;
        }

        return model;
    }

    public Task<FriendRequestResult> SendFriendRequestAsync(string recipientUsername) =>
        Task.FromResult<FriendRequestResult>(default);

    public Task<CancelFriendRequestResult> CancelFriendRequest(OutgoingFriendRequest friendRequest) =>
        Task.FromResult<CancelFriendRequestResult>(default);

    public Task<FriendRequestResponseResult> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted) =>
        Task.FromResult<FriendRequestResponseResult>(default);

    public Task<RemoveFriendResult> RemoveFriendAsync(UserModel userModel) =>
        Task.FromResult<RemoveFriendResult>(default);

    public Task RefreshFriendListAsync() => Task.CompletedTask;

    public Task RefreshFriendRequestsAsync() => Task.CompletedTask;

    public Task SetStatusAsync(UserStatus status) => Task.CompletedTask;
}
