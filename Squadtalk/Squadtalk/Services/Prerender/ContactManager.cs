using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Models;
using Shared.Reactive;
using Shared.Results;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ContactManager : LazyModelCreator, IContactManager
{
    private static readonly ObservableDictionary<UserId, UserModel> EmptyFriends = [];
    private static readonly ObservableDictionary<FriendRequestId, IncomingFriendRequest> EmptyIncomingFriendRequests = [];

    private readonly PrerenderPersistantState _prerenderPersistantState;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly Dictionary<UserId, UserModel> _userModels = [];

    private ObservableDictionary<UserId, UserModel>? _friends;
    private ObservableDictionary<FriendRequestId, IncomingFriendRequest>? _incomingFriendRequests;

    private ObservableDictionary<UserId, UserModel> LazyFriends => TryCreateModels(ref _friends, EmptyFriends);
    private ObservableDictionary<FriendRequestId, IncomingFriendRequest> LazyIncomingFriendRequests => TryCreateModels(ref _incomingFriendRequests, EmptyIncomingFriendRequests);

    public IObservableCollection<UserModel> FriendList => LazyFriends.Values;
    public IObservableCollection<IncomingFriendRequest> IncomingFriendRequests => LazyIncomingFriendRequests.Values;
    public IObservableCollection<OutgoingFriendRequest> OutgoingFriendRequests => ObservableCollection<OutgoingFriendRequest>.Empty;

    public UserStatus UserStatus => UserStatus.Unknown;

    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public UserModel LocalUserModel { get; }

    public ContactManager(
        PrerenderPersistantState prerenderPersistantState,
        IUserAuthenticationService userAuthenticationService) : base(prerenderPersistantState)
    {
        _prerenderPersistantState = prerenderPersistantState;
        _userAuthenticationService = userAuthenticationService;

        UserModelProvider = GetOrCreateUserModel;
        LocalUserModel = CreateLocalUserModel();
    }

    protected override void CreateModels(PrerenderPersistantState prerenderPersistantState)
    {
        var friends = _prerenderPersistantState.Friends ?? Array.Empty<UserDto>();
        _friends = friends.Select(UserModelProvider)
            .ToObservableDictionary(x => x.Id, x => x);

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

        _incomingFriendRequests = incomingFriendRequests.ToObservable();
    }

    private UserModel CreateLocalUserModel()
    {
        var model = new UserModel
        {
            Status = UserStatus.Unknown,
            Username = _userAuthenticationService.Username,
            Id = _userAuthenticationService.UserId,
            IsLocal = true
        };

        _userModels.TryAdd(model.Id, model);

        return model;
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
