using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Models;
using Shared.Reactive;
using Shared.Results;

namespace Shared.Services;

public interface IContactManager
{
    UserStatus UserStatus { get; }

    UserModel LocalUserModel { get; }

    Func<IChatUser, UserModel> UserModelProvider { get; }

    IObservableCollection<UserModel> FriendList { get; }

    IObservableCollection<IncomingFriendRequest> IncomingFriendRequests { get; }
    IObservableCollection<OutgoingFriendRequest> OutgoingFriendRequests { get; }

    UserModel? FindUserById(UserId userId);

    UserModel GetOrCreateUserModel(IChatUser chatUser);

    Task<FriendRequestResult> SendFriendRequestAsync(string recipientUsername);

    Task<CancelFriendRequestResult> CancelFriendRequest(OutgoingFriendRequest friendRequest);

    Task<FriendRequestResponseResult> RespondToFriendRequestAsync(IncomingFriendRequest friendRequest, bool accepted);

    Task<RemoveFriendResult> RemoveFriendAsync(UserModel userModel);

    Task RefreshFriendListAsync();

    Task RefreshFriendRequestsAsync();

    Task SetStatusAsync(UserStatus status);
}
