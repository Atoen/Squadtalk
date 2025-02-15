using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Signalr;
using Shared.Signalr.Clients;
using Squadtalk.Data;
using Squadtalk.Extensions;
using Squadtalk.Services;

namespace Squadtalk.Signalr;

public partial class AppHub
{
    private IVoiceChatClient VoiceClient(string connectionId) => Clients.Client(connectionId);
    private IVoiceChatClient VoiceGroup(string groupName) => Clients.Group(groupName);
    private IVoiceChatClient OthersInVoiceGroup(string groupName) => Clients.OthersInGroup(groupName);
    private IVoiceChatClient VoiceCaller => Clients.Caller;

    public bool Ping() => true;

    [HubMethodName(HubMethods.GetRtcEndpoint)]
    public string GetRtcEndpoint(IConfiguration configuration) => configuration.GetString("RtcEndpoint");

    [HubMethodName(HubMethods.StartVoiceCall)]
    public async Task<RoomTokenDto?> StartCall(
        GroupId groupId, VoiceCallManagerOld voiceCallManagerOld, LiveKitService liveKitService)
    {
        var participant = await GetParticipatingUserWithGroups(groupId);
        if (participant is null)
        {
            await VoiceCaller.CallFailed("Failed to create voice call");
            return null;
        }

        voiceCallManagerOld.VoiceCallInitiated(participant, groupId);

        var token = liveKitService.CreateRoomToken(participant, groupId);

        _logger.LogInformation("Token: {Token}", token.Token);

        return token;
    }

    [HubMethodName(HubMethods.AcceptCall)]
    public async Task<RoomTokenDto?> AcceptCall(
        GroupId groupId, VoiceCallManagerOld voiceCallManagerOld, LiveKitService liveKitService)
    {
        var participant = await GetParticipatingUserWithGroups(groupId);
        if (participant is null)
        {
            await VoiceCaller.CallFailed("Failed to join the call");
            return null;
        }

        voiceCallManagerOld.VoiceCallInitiated(participant, groupId);

        if (!voiceCallManagerOld.ChannelHasActiveCall(groupId))
        {
            return null;
        }

        await OthersInVoiceGroup(groupId).CallAccepted(groupId, participant.ToDto());

        return liveKitService.CreateRoomToken(participant, groupId);
    }

    [HubMethodName(HubMethods.DeclineCall)]
    public async Task DeclineCall(GroupId groupId, VoiceCallManagerOld voiceCallManagerOld)
    {
        var participant = await GetParticipatingUserWithGroups(groupId);
        if (participant is null)
        {
            return;
        }

        var room = voiceCallManagerOld.ActiveRooms.SingleOrDefault(x => x.GroupId == groupId);
        if (room is null)
        {
            return;
        }

        await OthersInVoiceGroup(groupId).CallDeclined(participant.ToDto(), groupId);
    }

    [HubMethodName(HubMethods.GroupHasActiveCall)]
    public async Task<bool> ChannelHasActiveCall(GroupId groupId, RtcConnectionManager connectionManager)
    {
        if (!await UserParticipatesInChannelAsync(groupId))
        {
            return false;
        }

        return await connectionManager.ChannelHasActiveCallAsync(groupId);
    }
}
