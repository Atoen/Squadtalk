using Shared.Data.TypedIds;
using Shared.DTOs.Chat;

namespace Shared.Signalr.Clients;

public interface IVoiceChatClient
{
    Task IncomingCall(GroupId groupId, UserId initiatorId);

    Task CallAccepted(GroupId groupId, UserDto accepting);

    Task CallDeclined(UserDto decliningUser, GroupId groupId);

    Task CallEnded(GroupId groupId);

    Task CallFailed(string reason);
}
