using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Signalr.Clients;
using Squadtalk.Data;
using Squadtalk.Services;

namespace Squadtalk.Signalr;

public partial class AppHub
{
    private IVoiceChatClient VoiceClient(string connectionId) => Clients.Client(connectionId);
    private IVoiceChatClient VoiceGroup(string groupName) => Clients.Group(groupName);
    private IVoiceChatClient OthersInVoiceGroup(string groupName) => Clients.OthersInGroup(groupName);
    private IVoiceChatClient VoiceCaller => Clients.Caller;

    public bool Ping() => true;

    public async Task<RoomTokenDto?> StartCall(
        GroupId groupId, VoiceCallManager voiceCallManager, LiveKitService liveKitService)
    {
        var participant = await GetParticipatingUserWithGroups(groupId);
        if (participant is null)
        {
            await VoiceCaller.CallFailed("Failed to create voice call");
            return null;
        }

        voiceCallManager.VoiceCallInitiated(participant, groupId);

        return liveKitService.CreateRoomToken(Context.User, groupId);
    }

    public async Task<RoomTokenDto?> AcceptCall(
        GroupId groupId, VoiceCallManager voiceCallManager, LiveKitService liveKitService)
    {
        var participant = await GetParticipatingUserWithGroups(groupId);
        if (participant is null)
        {
            await VoiceCaller.CallFailed("Failed to join the call");
            return null;
        }

        voiceCallManager.VoiceCallInitiated(participant, groupId);

        if (!voiceCallManager.ChannelHasActiveCall(groupId))
        {
            return null;
        }

        await OthersInVoiceGroup(groupId).CallAccepted(groupId, participant.ToDto());

        return liveKitService.CreateRoomToken(Context.User, groupId);
    }

    public async Task DeclineCall(GroupId groupId, VoiceCallManager voiceCallManager)
    {
        var participant = await GetParticipatingUserWithGroups(groupId);
        if (participant is null)
        {
            return;
        }

        var room = voiceCallManager.ActiveRooms.SingleOrDefault(x => x.GroupId == groupId);
        if (room is null)
        {
            return;
        }

        await OthersInVoiceGroup(groupId).CallDeclined(participant.ToDto(), groupId);
    }

    public async Task<bool> ChannelHasActiveCall(GroupId groupId, VoiceCallManager voiceCallManager)
    {
        var participant = await GetParticipatingUserWithGroups(groupId);
        if (participant is null)
        {
            return false;
        }

        return voiceCallManager.ChannelHasActiveCall(groupId);
    }
}
