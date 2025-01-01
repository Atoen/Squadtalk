using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Models;
using Shared.Results;
using Shared.Services;
using Squadtalk.Client.Network;

namespace Squadtalk.Client.Services;

internal class ContactManager : IContactManager
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IChatApi _chatApi;
    private readonly ILogger<ContactManager> _logger;

    private readonly Dictionary<UserId, UserModel> _users = [];

    private readonly Dictionary<FriendRequestId, IncomingFriendRequest> _incomingFriendRequests = [];
    private readonly Dictionary<FriendRequestId, OutgoingFriendRequest> _outgoingFriendRequests = [];

    public event Action? ContactsStateChanged;
    public event Action<UserModel>? ContactDisconnected;
    public event Action<UserModel>? ContactConnected;
    public event Action<IncomingFriendRequest>? FriendRequestReceived;

    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public IReadOnlyCollection<UserModel> AllContacts => _users.Values;
    public IReadOnlyCollection<UserModel> FriendList { get; } = [];
    public IReadOnlyCollection<UserModel> OtherContacts => _users.Values;

    public IReadOnlyCollection<IncomingFriendRequest> IncomingFriendRequests => _incomingFriendRequests.Values;
    public IReadOnlyCollection<OutgoingFriendRequest> OutgoingFriendRequests => _outgoingFriendRequests.Values;

    public ContactManager(
        SignalrService signalrService,
        IUserAuthenticationService userAuthenticationService,
        IChatApi chatApi,
        ILogger<ContactManager> logger)
    {
        _userAuthenticationService = userAuthenticationService;
        _chatApi = chatApi;
        _logger = logger;

        UserModelProvider = GetOrCreateUserModel;

        signalrService.UserConnected += UserConnected;
        signalrService.ConnectedUsersReceived += ReceivedConnectedUsers;
        signalrService.UserDisconnected += UserDisconnected;

        _outgoingFriendRequests.Add(new FriendRequestId(7), new OutgoingFriendRequest
        {
            Id = new FriendRequestId(7),
            To = UserModelProvider(new UserDto
            {
                Id = UserId.New,
                Username = "Agent Chaosu #1"
            }),
            CreatedAt = DateTimeOffset.Now.AddDays(-2)
        });

        _incomingFriendRequests.Add(new FriendRequestId(8), new IncomingFriendRequest
        {
            Id = new FriendRequestId(8),
            From = UserModelProvider(new UserDto
            {
                Id = UserId.New,
                Username = "Agent Chaosu #2"
            }),
            CreatedAt = DateTimeOffset.Now
        });

        _incomingFriendRequests.Add(new FriendRequestId(28), new IncomingFriendRequest
        {
            Id = new FriendRequestId(28),
            From = UserModelProvider(new UserDto
            {
                Id = UserId.New,
                Username = "Agent Chaosu #23"
            }),
            CreatedAt = DateTimeOffset.Now
        });

        _incomingFriendRequests.Add(new FriendRequestId(11), new IncomingFriendRequest
        {
            Id = new FriendRequestId(11),
            From = UserModelProvider(new UserDto
            {
                Id = UserId.New,
                Username = "Agent Chaosu #22"
            }),
            CreatedAt = DateTimeOffset.Now
        });
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
        try
        {
            var data = new FriendRequestDto
            {
                RecipientUsername = recipientUsername,
                RequestingUserId = _userAuthenticationService.UserId
            };

            return await _chatApi.SendFriendRequest(data);
        }
        catch
        {
            return null;
        }
    }

    public async Task<CancelFriendRequestResult?> CancelFriendRequest(OutgoingFriendRequest friendRequest)
    {
        try
        {
            var data = new CancelFriendRequestDto
            {
                RequestId = friendRequest.Id,
                CancellingUserId = _userAuthenticationService.UserId
            };

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted)
    {
        try
        {
            var data = new FriendRequestResponseDto
            {
                RespondingUserId = _userAuthenticationService.UserId,
                FriendRequestId = friendRequest.Id,
                Accepted = accepted
            };

            return await _chatApi.RespondToFriendRequest(data);
        }
        catch
        {
            return null;
        }
    }

    public async Task<RemoveFriendResult?> RemoveFriendAsync(UserId friendId)
    {
        try
        {
            var data = new RemoveFriendDto
            {
                FriendId = friendId
            };

            return await _chatApi.RemoveFriend(data);
        }
        catch
        {
            return null;
        }
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

    private void SetUserStatus(IChatUser user, UserStatus status)
    {
        var model = GetOrCreateUserModel(user);
        model.Status = status;
    }

    private Task UserConnected(UserDto user)
    {
        if (user.Id == _userAuthenticationService.UserId)
        {
            _logger.LogWarning("Connected user id is the same");
            return Task.CompletedTask;
        }

        SetUserStatus(user, UserStatus.Online);

        ContactsStateChanged?.Invoke();

        return Task.CompletedTask;
    }

    private Task ReceivedConnectedUsers(IEnumerable<UserDto> users, bool fromPersistedData)
    {
        foreach (var user in users)
        {
            UserConnected(user);
        }

        return Task.CompletedTask;
    }

    private Task UserDisconnected(UserDto user)
    {
        if (user.Id == _userAuthenticationService.UserId)
        {
            _logger.LogWarning("Disconnected user id is the same");
            return Task.CompletedTask;
        }

        if (_users.Remove(user.Id, out var disconnected))
        {
            ContactDisconnected?.Invoke(disconnected);
        }

        return Task.CompletedTask;
    }
}
