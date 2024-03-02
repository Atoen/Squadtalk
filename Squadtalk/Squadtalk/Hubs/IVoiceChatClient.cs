using Shared.DTOs;

namespace Squadtalk.Hubs;

public interface IVoiceChatClient
{
    Task CallOfferIncoming(VoiceCallDto voiceCallDto);

    Task UserJoinedCall(UserDto userDto, string callId);

    Task CallFailed(string? reason);
    
    Task ReceivePacket(VoicePacketDto voicePacketDto);
}