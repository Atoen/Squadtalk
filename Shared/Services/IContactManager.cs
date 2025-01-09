using Shared.Data;
using Shared.Enums;
using Shared.Models;
using Shared.Results;

namespace Shared.Services;

public interface IContactManager
{
    event Action? FriendListChanged;
    event Action? FriendRequestsChanged;
    event Action<IncomingFriendRequest>? FriendRequestReceived;

    Func<IChatUser, UserModel> UserModelProvider { get; }

    IReadOnlyCollection<UserModel> FriendList { get; }

    IReadOnlyCollection<IncomingFriendRequest> IncomingFriendRequests { get; }
    IReadOnlyCollection<OutgoingFriendRequest> OutgoingFriendRequests { get; }

    UserModel GetOrCreateUserModel(IChatUser chatUser);

    Task<FriendRequestResult?> SendFriendRequestAsync(string recipientUsername);

    Task<CancelFriendRequestResult?> CancelFriendRequest(OutgoingFriendRequest friendRequest);

    Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted);

    Task<RemoveFriendResult?> RemoveFriendAsync(UserModel userModel);

    Task RefreshFriendListAsync();

    Task RefreshFriendRequestsAsync();

    Task SetStatusAsync(UserStatus status);
}
