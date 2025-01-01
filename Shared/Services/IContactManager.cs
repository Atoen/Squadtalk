using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Models;
using Shared.Results;

namespace Shared.Services;

public interface IContactManager
{
    event Action? ContactsStateChanged;
    event Action<UserModel>? ContactDisconnected;
    event Action<UserModel>? ContactConnected;

    event Action<IncomingFriendRequest>? FriendRequestReceived;

    Func<IChatUser, UserModel> UserModelProvider { get; }

    IReadOnlyCollection<UserModel> AllContacts { get; }
    IReadOnlyCollection<UserModel> FriendList { get; }
    IReadOnlyCollection<UserModel> OtherContacts { get; }

    IReadOnlyCollection<IncomingFriendRequest> IncomingFriendRequests { get; }
    IReadOnlyCollection<OutgoingFriendRequest> OutgoingFriendRequests { get; }

    UserModel GetOrCreateUserModel(IChatUser chatUser);

    Task<FriendRequestResult?> SendFriendRequestAsync(string recipientUsername);

    Task<CancelFriendRequestResult?> CancelFriendRequest(OutgoingFriendRequest friendRequest);

    Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted);

    Task<RemoveFriendResult?> RemoveFriendAsync(UserId friendId);

    Task<List<UserModel>> GetFriendsAsync();

    Task<List<PendingFriendRequestDto>> GetPendingFriendRequestsAsync();
}
