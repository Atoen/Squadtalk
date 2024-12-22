using Shared.Data;
using Shared.Data.Results;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Network;

namespace Squadtalk.Client.Services;

internal class ContactManager : IContactManager
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IChatApi _chatApi;
    private readonly ILogger<ContactManager> _logger;

    private readonly Dictionary<UserId, UserModel> _users = [];

    public event Action? ContactsStateChanged;
    public event Action<UserModel>? ContactDisconnected;
    public event Action<UserModel>? ContactConnected;

    public Func<IChatUser, UserModel> UserModelProvider { get; }

    public IReadOnlyCollection<UserModel> AllContacts => _users.Values;
    public IReadOnlyCollection<UserModel> FriendList { get; } = [];
    public IReadOnlyCollection<UserModel> OtherContacts => _users.Values;

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

    public async Task<FriendRequestResult> SendFriendRequestAsync(FriendRequestDto friendRequest)
    {
        try
        {
            var result = await _chatApi.SendFriendRequest(friendRequest);
            return result.Successful switch
            {
                true => new FriendRequestResult.Sent(),
                false => new FriendRequestResult.NotFound()
            };
        }
        catch
        {
            return new FriendRequestResult.NetworkError();
        }
    }

    public Task AcceptFriendRequestAsync() => throw new NotImplementedException();
    public Task DeclineFriendRequestAsync() => throw new NotImplementedException();
    public Task RemoveFriendAsync() => throw new NotImplementedException();

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
