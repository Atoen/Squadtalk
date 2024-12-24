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

    Func<IChatUser, UserModel> UserModelProvider { get; }

    IReadOnlyCollection<UserModel> AllContacts { get; }
    IReadOnlyCollection<UserModel> FriendList { get; }
    IReadOnlyCollection<UserModel> OtherContacts { get; }

    UserModel GetOrCreateUserModel(IChatUser chatUser);

    Task<FriendRequestResult?> SendFriendRequestAsync(string recipientUsername);

    Task<CancelFriendRequestResult?> CancelFriendRequest(FriendRequestId requestId);

    Task<FriendRequestResponseResult?> RespondToFriendRequestAsync(FriendRequestId requestId, bool accepted);

    Task<RemoveFriendResult?> RemoveFriendAsync(UserId friendId);

    Task<List<UserModel>> GetFriendsAsync();

    Task<List<PendingFriendRequestDto>> GetPendingFriendRequestsAsync();
}
