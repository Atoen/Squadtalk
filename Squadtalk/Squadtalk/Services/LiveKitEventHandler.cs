using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.LiveKit.Events;
using Squadtalk.Hubs;

namespace Squadtalk.Services;

public class LiveKitEventHandler(
    IHubContext<ChatHub, IChatClient> hubContext,
    VoiceCallManager voiceCallManager,
    SystemMessageService systemMessageService,
    ApplicationDbContext dbContext,
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
        logger.LogInformation("Room {Room} started", dto.ChannelId);

        var initiatorId = voiceCallManager.AddRoom(dto);
        await hubContext.Clients.Group(dto.ChannelId).IncomingCall(dto.ChannelId, initiatorId);

        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == initiatorId);
        if (user is null)
        {
            logger.LogError("Call initiator not found");
            return;
        }

        await systemMessageService.SendCallStartedMessageAsync(user, dto.ChannelId, dto.CallId);
    }

    private async Task OnRoomFinished(RoomFinishedEvent roomFinishedEvent)
    {
        var dto = roomFinishedEvent.Room;
        var channelId = dto.ChannelId;

        var removedRoom = voiceCallManager.RemoveRoom(channelId, out var data);
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

        await voiceCallManager.AddParticipantAsync(participantDto, roomDto.ChannelId);
        logger.LogInformation("Participant {Participant} joined room {Room}", participantDto.Username, roomDto.ChannelId);
    }

    private async Task OnParticipantLeft(ParticipantLeftEvent participantLeftEvent)
    {
        var participantDto = participantLeftEvent.Participant;
        var roomDto = participantLeftEvent.Room;
        var channelId = roomDto.ChannelId;

        var roomIsEmpty = await voiceCallManager.RemoveParticipantAsync(participantDto, channelId);
        logger.LogInformation("Participant {Participant} left room {Room}", participantDto.Username, channelId);

        if (roomIsEmpty && voiceCallManager.RemoveRoom(channelId, out var data))
        {
            var (initiatorId, callDuration, callMissed) = data;
            await NotifyAboutCallEnded(channelId, initiatorId, callDuration, callMissed, roomDto.CallId);
        }
    }

    private async Task NotifyAboutCallEnded(ChannelId channelId, UserId callInitiatorId, TimeSpan callDuration, bool callMissed, string callId)
    {
        logger.LogInformation("Ending call on channel {ChannelId}", channelId);

        await hubContext.Clients.Group(channelId).CallEnded(channelId);
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == callInitiatorId);

        if (user is null)
        {
            logger.LogError("Call initiator not found");
            return;
        }

        await systemMessageService.SendCallEndedMessageAsync(user, channelId, callDuration, callMissed, callId);
    }
}
