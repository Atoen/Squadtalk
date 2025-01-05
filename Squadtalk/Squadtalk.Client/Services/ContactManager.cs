using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Models;
using Shared.Results;
using Shared.Services;
using Squadtalk.Client.Network;
using Squadtalk.Client.Services.SignalR;

namespace Squadtalk.Client.Services;

internal class ContactManager : IContactManager
{
    private readonly SignalrService _signalrService;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IChatApi _chatApi;
    private readonly NotificationService _notificationService;
    private readonly ILogger<ContactManager> _logger;

    private readonly Dictionary<UserId, UserModel> _users = [];
    private readonly Dictionary<UserId, UserModel> _friends = [];

    private readonly Dictionary<FriendRequestId, IncomingFriendRequest> _incomingFriendRequests = [];
    private readonly Dictionary<FriendRequestId, OutgoingFriendRequest> _outgoingFriendRequests = [];

    public event Action? FriendListChanged;
    public event Action? FriendRequestsChanged;
    public event Action<IncomingFriendRequest>? FriendRequestReceived;

    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public IReadOnlyCollection<UserModel> AllContacts => _users.Values;
    public IReadOnlyCollection<UserModel> FriendList => _friends.Values;
    public IReadOnlyCollection<UserModel> OtherContacts => _users.Values;

    public IReadOnlyCollection<IncomingFriendRequest> IncomingFriendRequests => _incomingFriendRequests.Values;
    public IReadOnlyCollection<OutgoingFriendRequest> OutgoingFriendRequests => _outgoingFriendRequests.Values;

    public ContactManager(
        SignalrService signalrService,
        IUserAuthenticationService userAuthenticationService,
        IChatApi chatApi,
        NotificationService notificationService,
        ILogger<ContactManager> logger)
    {
        _signalrService = signalrService;
        _userAuthenticationService = userAuthenticationService;
        _chatApi = chatApi;
        _notificationService = notificationService;
        _logger = logger;

        UserModelProvider = GetOrCreateUserModel;

        notificationService.FriendRequestAcceptedFromNotification += FriendAcceptedFromNotification;

        signalrService.FriendRequestReceived += ReceivedFriendRequest;
        signalrService.FriendAdded += FriendAdded;
        signalrService.FriendRemoved += FriendRemoved;
        signalrService.FriendRequestCancelled += FriendRequestCancelled;
        signalrService.FriendRequestResponded += FriendRequestResponded;
    }

    public UserModel GetOrCreateUserModel(IChatUser chatUser)
    {
        if (!_users.TryGetValue(chatUser.Id, out var model))
        {
            model = UserModel.Create(chatUser);
            _users[chatUser.Id] = model;
        }

        return model;
    }

    public async Task<FriendRequestResult?> SendFriendRequestAsync(string recipientUsername)
    {
        var result = await _signalrService.SendFriendRequestAsync(recipientUsername);
        if (result.Status != FriendRequestResult.Success)
        {
            return result.Status;
        }

        if (result.FriendRequest is not { } pendingFriendRequestDto)
        {
            _notificationService.ShowFailedToSendFriendRequestNotification(recipientUsername);
            return FriendRequestResult.Error;
        }

        var outgoingRequest = new OutgoingFriendRequest
        {
            CreatedAt = pendingFriendRequestDto.CreatedAt,
            To = GetOrCreateUserModel(pendingFriendRequestDto.Recipient),
            Id = pendingFriendRequestDto.Id
        };

        _outgoingFriendRequests.Add(outgoingRequest.Id, outgoingRequest);

        return FriendRequestResult.Success;
    }

    public async Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted)
    {
        var result = await _signalrService.RespondToFriendRequestAsync(friendRequest.Id, accepted);
        if (result is FriendRequestResponseResult.SuccessAccepted or FriendRequestResponseResult.SuccessRejected)
        {
            _incomingFriendRequests.Remove(friendRequest.Id);
            FriendRequestsChanged?.Invoke();
        }
        else
        {
            _notificationService.ShowFailedToRespondToFriendRequestNotification(friendRequest);
        }

        return result;
    }

    public async Task<CancelFriendRequestResult?> CancelFriendRequest(OutgoingFriendRequest friendRequest)
    {
        var ok = await _signalrService.CancelFriendRequestAsync(friendRequest.Id);
        if (ok)
        {
            _outgoingFriendRequests.Remove(friendRequest.Id);
        }
        else
        {
            _notificationService.ShowFailedToCancelFriendRequestNotification(friendRequest);
        }

        return ok ? CancelFriendRequestResult.Success : CancelFriendRequestResult.InvalidRequest;
    }

    public async Task<RemoveFriendResult?> RemoveFriendAsync(UserModel friend)
    {
        var result = await _signalrService.RemoveFriendAsync(friend.Id);
        if (result == RemoveFriendResult.Success)
        {
            _friends.Remove(friend.Id);
        }
        else
        {
            _notificationService.ShowFailedToRemoveFriendNotification(friend);
        }

        return result;
    }

    public async Task<List<UserModel>> GetFriendsAsync()
    {
        try
        {
            var result = await _chatApi.GetFriends();
            return result.Select(GetOrCreateUserModel).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when fetching friend list");
            return [];
        }
    }

    public async Task<List<PendingFriendRequestDto>> GetPendingFriendRequestsAsync()
    {
        try
        {
            var result = await _chatApi.GetPendingFriendRequests();

            foreach (var requestDto in result)
            {
                if (requestDto.Requester.Id == _userAuthenticationService.UserId)
                {
                    var outgoingRequest = new OutgoingFriendRequest
                    {
                        Id = requestDto.Id,
                        CreatedAt = requestDto.CreatedAt,
                        To = GetOrCreateUserModel(requestDto.Recipient)
                    };

                    _outgoingFriendRequests.Add(outgoingRequest.Id, outgoingRequest);
                }
                else if (requestDto.Recipient.Id == _userAuthenticationService.UserId)
                {
                    var incomingRequest = new IncomingFriendRequest
                    {
                        Id = requestDto.Id,
                        CreatedAt = requestDto.CreatedAt,
                        From = GetOrCreateUserModel(requestDto.Requester)
                    };

                    _incomingFriendRequests.Add(incomingRequest.Id, incomingRequest);
                }
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when fetching pending friend requests");
            return [];
        }
    }

    private async Task FriendAcceptedFromNotification(IncomingFriendRequest incomingFriendRequest)
    {
        var result = await RespondToFriendRequestAsync(incomingFriendRequest, true);
        if (result is FriendRequestResponseResult.SuccessAccepted or FriendRequestResponseResult.SuccessRejected)
        {
            _incomingFriendRequests.Remove(incomingFriendRequest.Id);
            FriendRequestsChanged?.Invoke();
        }
    }

    private void FriendAdded(UserDto friend)
    {
        var friendModel = GetOrCreateUserModel(friend);
        _friends.Add(friendModel.Id, friendModel);
        FriendListChanged?.Invoke();
    }

    private void FriendRemoved(UserId friendId)
    {
        if (_friends.Remove(friendId))
        {
            FriendListChanged?.Invoke();
        }
    }

    private void ReceivedFriendRequest(PendingFriendRequestDto friendRequest)
    {
        var incomingFriendRequest = new IncomingFriendRequest
        {
            Id = friendRequest.Id,
            CreatedAt = friendRequest.CreatedAt,
            From = GetOrCreateUserModel(friendRequest.Requester)
        };

        _incomingFriendRequests.Add(incomingFriendRequest.Id, incomingFriendRequest);
        _notificationService.ShowIncomingFriendRequestNotification(incomingFriendRequest);

        FriendRequestReceived?.Invoke(incomingFriendRequest);
        FriendRequestsChanged?.Invoke();
    }

    private void FriendRequestCancelled(FriendRequestId friendRequestId)
    {
        if (_incomingFriendRequests.Remove(friendRequestId) ||
            _outgoingFriendRequests.Remove(friendRequestId))
        {
            FriendRequestsChanged?.Invoke();
        }
    }

    private void FriendRequestResponded(FriendRequestResponseDto response)
    {
        if (!_outgoingFriendRequests.Remove(response.FriendRequestId, out var request))
        {
            return;
        }

        if (response.Accepted)
        {
            _notificationService.ShowUserAcceptedFriendRequestNotification(request);
        }

        FriendRequestsChanged?.Invoke();
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
}
