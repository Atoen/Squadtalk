using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Signalr.Clients;
using Squadtalk.Data;
using Squadtalk.Services;

namespace Squadtalk.Signalr;

partial class AppHub
{
    private IVoiceChatClient VoiceClient(string connectionId) => Clients.Client(connectionId);
    private IVoiceChatClient VoiceGroup(string groupName) => Clients.Group(groupName);
    private IVoiceChatClient OthersInVoiceGroup(string groupName) => Clients.OthersInGroup(groupName);
    private IVoiceChatClient VoiceCaller => Clients.Caller;

    public bool Ping() => true;

    public async Task<RoomTokenDto?> StartCall(
        ChannelId channelId, VoiceCallManager voiceCallManager, LiveKitService liveKitService)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            await VoiceCaller.CallFailed("Failed to create voice call");
            return null;
        }

        voiceCallManager.VoiceCallInitiated(participant, channelId);

        return liveKitService.CreateRoomToken(Context.User, channelId);
    }

    public async Task<RoomTokenDto?> AcceptCall(
        ChannelId channelId, VoiceCallManager voiceCallManager, LiveKitService liveKitService)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            await VoiceCaller.CallFailed("Failed to join the call");
            return null;
        }

        voiceCallManager.VoiceCallInitiated(participant, channelId);

        if (!voiceCallManager.ChannelHasActiveCall(channelId))
        {
            return null;
        }

        await OthersInVoiceGroup(channelId).CallAccepted(channelId, participant.ToDto());

        return liveKitService.CreateRoomToken(Context.User, channelId);
    }

    public async Task DeclineCall(ChannelId channelId, VoiceCallManager voiceCallManager)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            return;
        }

        var room = voiceCallManager.ActiveRooms.SingleOrDefault(x => x.ChannelId == channelId);
        if (room is null)
        {
            return;
        }

        await OthersInVoiceGroup(channelId).CallDeclined(participant.ToDto(), channelId);
    }

    public async Task<bool> ChannelHasActiveCall(ChannelId channelId, VoiceCallManager voiceCallManager)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            return false;
        }

        return voiceCallManager.ChannelHasActiveCall(channelId);
    }
}
