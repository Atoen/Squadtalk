using Microsoft.AspNetCore.Authorization;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Squadtalk.Data;

namespace Squadtalk.Hubs;

[Authorize]
public partial class ChatHub
{
    public bool Ping() => true;

    public async Task<RoomTokenDto?> StartCall(ChannelId channelId)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            await VoiceCaller.CallFailed("Failed to create voice call");
            return null;
        }

        _voiceCallManager.VoiceCallInitiated(participant, channelId);

        return _liveKitService.CreateRoomToken(Context.User, channelId);
    }

    public async Task<RoomTokenDto?> AcceptCall(ChannelId channelId)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            await VoiceCaller.CallFailed("Failed to join the call");
            return null;
        }

        _voiceCallManager.VoiceCallInitiated(participant, channelId);

        if (!_voiceCallManager.ChannelHasActiveCall(channelId))
        {
            return null;
        }

        await OthersInVoiceGroup(channelId).CallAccepted(channelId, participant.ToDto());

        return _liveKitService.CreateRoomToken(Context.User, channelId);
    }

    public async Task DeclineCall(ChannelId channelId)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            return;
        }

        var room = _voiceCallManager.ActiveRooms.SingleOrDefault(x => x.ChannelId == channelId);
        if (room is null)
        {
            return;
        }

        await OthersInVoiceGroup(channelId).CallDeclined(participant.ToDto(), channelId);
    }

    public async Task<bool> ChannelHasActiveCall(ChannelId channelId)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            return false;
        }

        return _voiceCallManager.ChannelHasActiveCall(channelId);
    }
}
