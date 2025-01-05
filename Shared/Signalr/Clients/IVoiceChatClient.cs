using Shared.Data.TypedIds;
using Shared.DTOs.Chat;

namespace Shared.Signalr.Clients;

public interface IVoiceChatClient
{
    Task IncomingCall(ChannelId channelId, UserId initiatorId);

    Task CallAccepted(ChannelId channelId, UserDto accepting);

    Task CallDeclined(UserDto decliningUser, ChannelId channelId);
    
    Task CallEnded(ChannelId channelId);

    Task CallFailed(string reason);
}
