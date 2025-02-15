using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Squadtalk.Data.LiveKit.Events;
using Squadtalk.Data.Repositories;
using Squadtalk.Signalr;

namespace Squadtalk.Services;

public class LiveKitEventHandler(
    IHubContext<AppHub, IChatClient> hubContext,
    VoiceCallManagerOld voiceCallManagerOld,
    SystemMessageService systemMessageService,
    ChatUserRepository chatUserRepository,
    ILogger<LiveKitEventHandler> logger)
{
    public Task HandleEventAsync(LiveKitEvent liveKitEvent)
    {
        logger.LogInformation("LiveKit event: {Event}", liveKitEvent.EventName);

        return liveKitEvent switch
        {
            RoomStartedEvent roomStartedEvent => OnRoomStarted(roomStartedEvent),
            RoomFinishedEvent roomFinishedEvent => OnRoomFinished(roomFinishedEvent),
            ParticipantJoinedEvent participantJoinedEvent => OnParticipantJoined(participantJoinedEvent),
            ParticipantLeftEvent participantLeftEvent => OnParticipantLeft(participantLeftEvent),
            _ => Task.CompletedTask
        };
    }

    private async Task OnRoomStarted(RoomStartedEvent roomStartedEvent)
    {
        var dto = roomStartedEvent.Room;
        logger.LogInformation("Room {Room} started", dto.GroupId);

        var initiatorId = voiceCallManagerOld.AddRoom(dto);
        await hubContext.Clients.Group(dto.GroupId).IncomingCall(dto.GroupId, initiatorId);

        var user = await chatUserRepository.FindUserByIdAsync(initiatorId);
        if (user is null)
        {
            logger.LogError("Call initiator not found");
            return;
        }

        await systemMessageService.SendCallStartedMessageAsync(user, dto.GroupId, dto.CallId);
    }

    private async Task OnRoomFinished(RoomFinishedEvent roomFinishedEvent)
    {
        var dto = roomFinishedEvent.Room;
        var channelId = dto.GroupId;

        var removedRoom = voiceCallManagerOld.RemoveRoom(channelId, out var data);
        if (!removedRoom)
        {
            logger.LogInformation("Room {Room} finished. Call already ended", channelId);
            return;
        }

        logger.LogInformation("Room {Room} finished", channelId);

        var (initiatorId, callDuration, callMissed) = data;
        await NotifyAboutCallEnded(channelId, initiatorId, callDuration, callMissed, dto.CallId);
    }

    private async Task OnParticipantJoined(ParticipantJoinedEvent participantJoinedEvent)
    {
        var participantDto = participantJoinedEvent.Participant;
        var roomDto = participantJoinedEvent.Room;

        await voiceCallManagerOld.AddParticipantAsync(participantDto, roomDto.GroupId);
        logger.LogInformation("Participant {Participant} joined room {Room}", participantDto.Username, roomDto.GroupId);
    }

    private async Task OnParticipantLeft(ParticipantLeftEvent participantLeftEvent)
    {
        var participantDto = participantLeftEvent.Participant;
        var roomDto = participantLeftEvent.Room;
        var channelId = roomDto.GroupId;

        var roomIsEmpty = await voiceCallManagerOld.RemoveParticipantAsync(participantDto, channelId);
        logger.LogInformation("Participant {Participant} left room {Room}", participantDto.Username, channelId);

        if (roomIsEmpty && voiceCallManagerOld.RemoveRoom(channelId, out var data))
        {
            var (initiatorId, callDuration, callMissed) = data;
            await NotifyAboutCallEnded(channelId, initiatorId, callDuration, callMissed, roomDto.CallId);
        }
    }

    private async Task NotifyAboutCallEnded(GroupId groupId, UserId callInitiatorId, TimeSpan callDuration, bool callMissed, string callId)
    {
        logger.LogInformation("Ending call on channel {ChannelId}", groupId);

        await hubContext.Clients.Group(groupId).CallEnded(groupId);
        var user = await chatUserRepository.FindUserByIdAsync(callInitiatorId);

        if (user is null)
        {
            logger.LogError("Call initiator not found");
            return;
        }

        await systemMessageService.SendCallEndedMessageAsync(user, groupId, callDuration, callMissed, callId);
    }
}
