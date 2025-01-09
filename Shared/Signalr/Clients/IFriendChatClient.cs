using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;

namespace Shared.Signalr.Clients;

public interface IFriendChatClient
{
    Task FriendRequestCreated(PendingFriendRequestDto friendRequestDto);

    Task FriendRequestCancelled(FriendRequestId friendRequestId);

    Task FriendRequestResponded(FriendRequestResponseDto friendRequestResponseDto);

    Task FriendAdded(UserDto friend);

    Task FriendRemoved(UserId friendId);

    Task FriendStatusChanged(UserId friendId, UserStatus status);
}
