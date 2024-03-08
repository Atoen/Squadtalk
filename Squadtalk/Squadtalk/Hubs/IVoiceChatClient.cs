using Shared.Data;
using Shared.DTOs;

namespace Squadtalk.Hubs;

public interface IVoiceChatClient
{
    Task IncomingCall(UserDto caller, CallOfferId id);

    Task CallAccepted(CallOfferId id);

    Task CallDeclined(CallOfferId id);
    
    Task CallEnded(CallId id);

    Task CallFailed(string reason);

    Task GetCallUsers(List<UserDto> users, CallId id);
}