using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Results;
using Squadtalk.Client.Data;

namespace Squadtalk.Client.Services.SignalR.Interfaces;

public interface ISignalrFriendService
{
    event Action? UserStatusChanged;

    event Action<PendingFriendRequestDto>? FriendRequestCreated;
    event Action<FriendRequestId>? FriendRequestCancelled;
    event Action<FriendRequestResponseDto>? FriendRequestResponded;

    event Action<UserDto>? FriendAdded;
    event Action<UserId>? FriendRemoved;

    event Action<List<UserDto>>? FriendListReceived;
    event Action<List<PendingFriendRequestDto>>? FriendRequestsReceived;
    event Action<UserId, UserStatus>? FriendStatusChanged;

    Task<NetworkResult<FriendRequestResult>> SendFriendRequestAsync(string recipientUsername);

    Task<NetworkResult<bool>> CancelFriendRequestAsync(FriendRequestId friendRequestId);

    Task<NetworkResult<FriendRequestResponseResult>> RespondToFriendRequestAsync(FriendRequestId friendRequestId, bool isAccepted);

    Task<NetworkResult<RemoveFriendResult>> RemoveFriendAsync(UserId friendId);

    Task<NetworkResult<List<UserDto>>> GetFriendListAsync();

    Task<NetworkResult<List<PendingFriendRequestDto>>> GetFriendRequestsAsync();

}
