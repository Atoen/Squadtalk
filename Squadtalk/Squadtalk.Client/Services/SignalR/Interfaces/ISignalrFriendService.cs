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

    Task<SignalrResult<FriendRequestResult>> SendFriendRequestAsync(string recipientUsername);

    Task<SignalrResult<bool>> CancelFriendRequestAsync(FriendRequestId friendRequestId);

    Task<SignalrResult<FriendRequestResponseResult>> RespondToFriendRequestAsync(FriendRequestId friendRequestId, bool isAccepted);

    Task<SignalrResult<RemoveFriendResult>> RemoveFriendAsync(UserId friendId);

    Task<SignalrResult<List<UserDto>>> GetFriendListAsync();

    Task<SignalrResult<List<PendingFriendRequestDto>>> GetFriendRequestsAsync();

}
