using Shared.Data.TypedIds;
using Shared.DTOs.Chat;

namespace Shared.Signalr.Clients;

public interface IChatClient : ITextChatClient, IVoiceChatClient
{
    Task FriendRequestCreated(PendingFriendRequestDto friendRequestDto);

    Task FriendRequestCancelled(FriendRequestId friendRequestId);

    Task FriendRequestResponded(FriendRequestResponseDto friendRequestResponseDto);

    Task FriendAdded(UserDto friend);

    Task FriendRemoved(UserId friendId);
}
