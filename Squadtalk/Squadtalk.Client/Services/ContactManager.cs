using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Models;
using Shared.Reactive;
using Shared.Results;
using Shared.Services;
using Squadtalk.Client.Services.SignalR;

namespace Squadtalk.Client.Services;

internal class ContactManager : IContactManager
{
    private readonly SignalrService _signalrService;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly NotificationService _notificationService;
    private readonly ILogger<ContactManager> _logger;

    private readonly Dictionary<UserId, UserModel> _userModels = [];
    private readonly ObservableDictionary<UserId, UserModel> _friends = [];

    private readonly ObservableDictionary<FriendRequestId, IncomingFriendRequest> _incomingFriendRequests = [];
    private readonly ObservableDictionary<FriendRequestId, OutgoingFriendRequest> _outgoingFriendRequests = [];

    public UserStatus UserStatus => _signalrService.UserStatus;
    public UserModel LocalUserModel { get; }

    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public IObservableCollection<UserModel> FriendList => _friends.Values;

    public IObservableCollection<IncomingFriendRequest> IncomingFriendRequests => _incomingFriendRequests.Values;
    public IObservableCollection<OutgoingFriendRequest> OutgoingFriendRequests => _outgoingFriendRequests.Values;

    public ContactManager(
        SignalrService signalrService,
        IUserAuthenticationService userAuthenticationService,
        NotificationService notificationService,
        ILogger<ContactManager> logger)
    {
        _signalrService = signalrService;
        _userAuthenticationService = userAuthenticationService;
        _notificationService = notificationService;
        _logger = logger;

        UserModelProvider = GetOrCreateUserModel;
        LocalUserModel = CreateLocalUserModel();

        userAuthenticationService.LocalUsernameChanged += LocalUsernameChanged;
        notificationService.FriendRequestAcceptedFromNotification += FriendAcceptedFromNotification;

        signalrService.FriendRequestCreated += CreatedFriendRequest;
        signalrService.FriendAdded += FriendAdded;
        signalrService.FriendRemoved += FriendRemoved;
        signalrService.FriendRequestCancelled += FriendRequestCancelled;
        signalrService.FriendRequestResponded += FriendRequestResponded;
        signalrService.FriendListReceived += FriendListReceived;
        signalrService.FriendRequestsReceived += FriendRequestsReceived;
        signalrService.FriendStatusChanged += FriendStatusChanged;
        signalrService.UserStatusChanged += LocalStatusChanged;
    }

    #region Public Methods

    public UserModel? FindUserById(UserId userId) => _userModels.GetValueOrDefault(userId);

    public UserModel GetOrCreateUserModel(IChatUser chatUser)
    {
        if (!_userModels.TryGetValue(chatUser.Id, out var model))
        {
            model = UserModel.Create(chatUser, _userAuthenticationService.UserId);
            _userModels[chatUser.Id] = model;
        }

        if (chatUser.Status != UserStatus.Unknown)
        {
            model.Status = chatUser.Status;
        }

        return model;
    }

    public async Task<FriendRequestResult> SendFriendRequestAsync(string recipientUsername)
    {
        var result = await _signalrService.SendFriendRequestAsync(recipientUsername);
        if (result.ErrorOrValueIs(FriendRequestResult.Error))
        {
            _notificationService.FailedToSendFriendRequest(recipientUsername);
            return FriendRequestResult.Error;
        }

        return result.Value;
    }

    public async Task<FriendRequestResponseResult> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted)
    {
        var result = await _signalrService.RespondToFriendRequestAsync(friendRequest.Id, accepted);
        if (result.ErrorOrValueIs(FriendRequestResponseResult.Error))
        {
            _notificationService.FailedToRespondToFriendRequest(friendRequest);
            return FriendRequestResponseResult.Error;
        }

        return result.Value;
    }

    public async Task<CancelFriendRequestResult> CancelFriendRequest(OutgoingFriendRequest friendRequest)
    {
        var result = await _signalrService.CancelFriendRequestAsync(friendRequest.Id);
        if (result.ErrorOrValueIs(false))
        {
            _notificationService.FailedToCancelFriendRequest(friendRequest);
            return CancelFriendRequestResult.Error;
        }

        return result.Value ? CancelFriendRequestResult.Success : CancelFriendRequestResult.InvalidRequest;
    }

    public async Task<RemoveFriendResult> RemoveFriendAsync(UserModel friend)
    {
        var result = await _signalrService.RemoveFriendAsync(friend.Id);
        if (result.ErrorOrValueIsNot(RemoveFriendResult.Success))
        {
            _notificationService.FailedToRemoveFriend(friend);
            return RemoveFriendResult.Error;
        }

        return result.Value;
    }

    public async Task RefreshFriendListAsync()
    {
        var result = await _signalrService.GetFriendListAsync();
        if (result.IsError || result.Value is not { Count: > 0 } friends)
        {
            return;
        }

        var models = friends.Select(UserModelProvider);
        _friends.Refresh(models);
    }

    public async Task RefreshFriendRequestsAsync()
    {
        var result = await _signalrService.GetFriendRequestsAsync();
        if (result.IsError || result.Value is not { Count: > 0 } requests)
        {
            return;
        }
        
        using var scope1 = new NotificationScope(_outgoingFriendRequests);
        using var scope2 = new NotificationScope(_incomingFriendRequests);

        _incomingFriendRequests.Clear();
        _outgoingFriendRequests.Clear();

        foreach (var request in requests)
        {
            AddFriendRequest(request);
        }
    }

    public async Task SetStatusAsync(UserStatus status)
    {
        await _signalrService.SetStatusAsync(status);
    }

    #endregion

    #region Event Handlers
    
    private void LocalUsernameChanged()
    {
        LocalUserModel.Username = _userAuthenticationService.Username;
    }

    private async Task FriendAcceptedFromNotification(IncomingFriendRequest incomingFriendRequest)
    {
        var result = await RespondToFriendRequestAsync(incomingFriendRequest, true);
        if (result is FriendRequestResponseResult.SuccessAccepted)
        {
            _incomingFriendRequests.Remove(incomingFriendRequest.Id);
        }
    }

    private void FriendAdded(UserDto friend) => _friends.Add(GetOrCreateUserModel(friend));

    private void FriendRemoved(UserId friendId) => _friends.Remove(friendId);

    private void CreatedFriendRequest(PendingFriendRequestDto friendRequest) => AddFriendRequest(friendRequest, invokeEvents: true);

    private void FriendRequestCancelled(FriendRequestId friendRequestId)
    {
        _ = _incomingFriendRequests.Remove(friendRequestId) || _outgoingFriendRequests.Remove(friendRequestId);
    }

    private void FriendRequestResponded(FriendRequestResponseDto response)
    {
        if (_incomingFriendRequests.Remove(response.FriendRequestId))
        {
            return;
        }

        if (_outgoingFriendRequests.Remove(response.FriendRequestId, out var request))
        {
            if (response.Accepted)
            {
                _notificationService.UserAcceptedFriendRequest(request);
            }
        }
    }

    private void FriendListReceived(List<UserDto> friends)
    {
        foreach (var friend in friends)
        {
            var model = GetOrCreateUserModel(friend);
            _friends[model.Id] = model;
        }
    }

    private void FriendRequestsReceived(List<PendingFriendRequestDto> friendRequests)
    {
        foreach (var friendRequest in friendRequests)
        {
            AddFriendRequest(friendRequest);
        }
    }

    private void FriendStatusChanged(UserId friendId, UserStatus status)
    {
        _logger.LogInformation("Friend status changed");

        if (!_friends.TryGetValue(friendId, out var friend))
        {
            return;
        }

        _logger.LogInformation("Changing status of {User} to {Status}", friend.Username, status);

        friend.Status = status;
    }
    
    private void LocalStatusChanged()
    {
        LocalUserModel.Status = _signalrService.UserStatus;
    }

    #endregion

    private UserModel CreateLocalUserModel()
    {
        var model = new UserModel
        {
            Status = _signalrService.UserStatus,
            Username = _userAuthenticationService.Username,
            Id = _userAuthenticationService.UserId,
            IsLocal = true,
            AvatarUrl = "user.png"
        };

        _userModels.TryAdd(model.Id, model);

        return model;
    }

    private bool AddFriendRequest(PendingFriendRequestDto requestDto, bool invokeEvents = false)
    {
        var currentUserId = _userAuthenticationService.UserId;
        if (requestDto.Requester.Id == currentUserId)
        {
            var outgoingRequest = new OutgoingFriendRequest
            {
                Id = requestDto.Id,
                CreatedAt = requestDto.CreatedAt,
                To = GetOrCreateUserModel(requestDto.Recipient)
            };

            return _outgoingFriendRequests.Add(outgoingRequest);
        }

        if (requestDto.Recipient.Id == currentUserId)
        {
            var incomingRequest = new IncomingFriendRequest
            {
                Id = requestDto.Id,
                CreatedAt = requestDto.CreatedAt,
                From = GetOrCreateUserModel(requestDto.Requester)
            };

            var added = _incomingFriendRequests.Add(incomingRequest);
            if (invokeEvents && added)
            {
                _notificationService.IncomingFriendRequest(incomingRequest);
            }

            return added;
        }

        return false;
    }
}
