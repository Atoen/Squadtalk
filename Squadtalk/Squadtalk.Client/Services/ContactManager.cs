using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Models;
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
    private readonly Dictionary<UserId, UserModel> _friends = [];

    private readonly Dictionary<FriendRequestId, IncomingFriendRequest> _incomingFriendRequests = [];
    private readonly Dictionary<FriendRequestId, OutgoingFriendRequest> _outgoingFriendRequests = [];

    public event Action? FriendListChanged;
    public event Action? FriendRequestsChanged;
    public event Action<IncomingFriendRequest>? FriendRequestReceived;
    public event Action? StatusChanged
    {
        add => _signalrService.UserStatusChanged += value;
        remove => _signalrService.UserStatusChanged -= value;
    }

    public UserStatus UserStatus => _signalrService.UserStatus;

    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public IReadOnlyCollection<UserModel> FriendList => _friends.Values;

    public IReadOnlyCollection<IncomingFriendRequest> IncomingFriendRequests => _incomingFriendRequests.Values;
    public IReadOnlyCollection<OutgoingFriendRequest> OutgoingFriendRequests => _outgoingFriendRequests.Values;

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

        notificationService.FriendRequestAcceptedFromNotification += FriendAcceptedFromNotification;

        signalrService.FriendRequestCreated += CreatedFriendRequest;
        signalrService.FriendAdded += FriendAdded;
        signalrService.FriendRemoved += FriendRemoved;
        signalrService.FriendRequestCancelled += FriendRequestCancelled;
        signalrService.FriendRequestResponded += FriendRequestResponded;
        signalrService.FriendListReceived += FriendListReceived;
        signalrService.FriendRequestsReceived += FriendRequestsReceived;
        signalrService.FriendStatusChanged += FriendStatusChanged;
    }

    #region PublicMethods

    public UserModel GetOrCreateUserModel(IChatUser chatUser)
    {
        if (!_userModels.TryGetValue(chatUser.Id, out var model))
        {
            model = UserModel.Create(chatUser);
            _userModels[chatUser.Id] = model;
        }

        model.Status = chatUser.Status;

        return model;
    }

    public async Task<FriendRequestResult?> SendFriendRequestAsync(string recipientUsername)
    {
        var result = await _signalrService.SendFriendRequestAsync(recipientUsername);
        if (result.ErrorOrValueIs(FriendRequestResult.Error))
        {
            _notificationService.ShowFailedToSendFriendRequestNotification(recipientUsername);
        }

        return result.Value;
    }

    public async Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted)
    {
        var result = await _signalrService.RespondToFriendRequestAsync(friendRequest.Id, accepted);
        if (result.ErrorOrValueIs(FriendRequestResponseResult.Error))
        {
            _notificationService.ShowFailedToRespondToFriendRequestNotification(friendRequest);
        }

        return result.Value;
    }

    public async Task<CancelFriendRequestResult?> CancelFriendRequest(OutgoingFriendRequest friendRequest)
    {
        var result = await _signalrService.CancelFriendRequestAsync(friendRequest.Id);
        if (result.ErrorOrValueIs(false))
        {
            _notificationService.ShowFailedToCancelFriendRequestNotification(friendRequest);
        }

        return result.Value ? CancelFriendRequestResult.Success : CancelFriendRequestResult.InvalidRequest;
    }

    public async Task<RemoveFriendResult?> RemoveFriendAsync(UserModel friend)
    {
        var result = await _signalrService.RemoveFriendAsync(friend.Id);
        if (result.ErrorOrValueIsNot(RemoveFriendResult.Error))
        {
            _notificationService.ShowFailedToRemoveFriendNotification(friend);
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

        var models = friends.Select(UserModelProvider).ToList();

        _friends.Clear();
        foreach (var model in models)
        {
            _friends.TryAdd(model.Id, model);
        }

        FriendListChanged?.Invoke();
    }

    public async Task RefreshFriendRequestsAsync()
    {
        var result = await _signalrService.GetFriendRequestsAsync();
        if (result.IsError || result.Value is not { Count: > 0 } requests)
        {
            return;
        }

        _incomingFriendRequests.Clear();
        _outgoingFriendRequests.Clear();

        foreach (var request in requests)
        {
            AddFriendRequest(request);
        }

        FriendRequestsChanged?.Invoke();
    }

    public async Task SetStatusAsync(UserStatus status)
    {
        await _signalrService.SetStatusAsync(status);
    }

    #endregion

    #region EventHandlers

    private async Task FriendAcceptedFromNotification(IncomingFriendRequest incomingFriendRequest)
    {
        var result = await RespondToFriendRequestAsync(incomingFriendRequest, true);
        if (result is FriendRequestResponseResult.SuccessAccepted)
        {
            _incomingFriendRequests.Remove(incomingFriendRequest.Id);
            FriendRequestsChanged?.Invoke();
        }
    }

    private void FriendAdded(UserDto friend)
    {
        var friendModel = GetOrCreateUserModel(friend);
        if (_friends.TryAdd(friendModel.Id, friendModel))
        {
            FriendListChanged?.Invoke();
        }
    }

    private void FriendRemoved(UserId friendId)
    {
        if (_friends.Remove(friendId))
        {
            FriendListChanged?.Invoke();
        }
    }

    private void CreatedFriendRequest(PendingFriendRequestDto friendRequest)
    {
        if (AddFriendRequest(friendRequest, invokeEvents: true))
        {
            FriendRequestsChanged?.Invoke();
        }
    }

    private void FriendRequestCancelled(FriendRequestId friendRequestId)
    {
        if (_incomingFriendRequests.Remove(friendRequestId) || _outgoingFriendRequests.Remove(friendRequestId))
        {
            FriendRequestsChanged?.Invoke();
        }
    }

    private void FriendRequestResponded(FriendRequestResponseDto response)
    {
        if (_incomingFriendRequests.Remove(response.FriendRequestId))
        {
            FriendRequestsChanged?.Invoke();
        }
        else if (_outgoingFriendRequests.Remove(response.FriendRequestId, out var request))
        {
            FriendRequestsChanged?.Invoke();
            if (response.Accepted)
            {
                _notificationService.ShowUserAcceptedFriendRequestNotification(request);
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

        FriendListChanged?.Invoke();
    }

    private void FriendRequestsReceived(List<PendingFriendRequestDto> friendRequests)
    {
        foreach (var friendRequest in friendRequests)
        {
            AddFriendRequest(friendRequest);
        }

        FriendRequestsChanged?.Invoke();
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

        FriendListChanged?.Invoke();
    }

    #endregion

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

            return _outgoingFriendRequests.TryAdd(outgoingRequest.Id, outgoingRequest);
        }

        if (requestDto.Recipient.Id == currentUserId)
        {
            var incomingRequest = new IncomingFriendRequest
            {
                Id = requestDto.Id,
                CreatedAt = requestDto.CreatedAt,
                From = GetOrCreateUserModel(requestDto.Requester)
            };

            var added = _incomingFriendRequests.TryAdd(incomingRequest.Id, incomingRequest);
            if (invokeEvents && added)
            {
                _notificationService.ShowIncomingFriendRequestNotification(incomingRequest);
                FriendRequestReceived?.Invoke(incomingRequest);
            }

            return added;
        }

        return false;
    }
}
