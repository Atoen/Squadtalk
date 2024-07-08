using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Shared.Data;
using Shared.Data.TypedIds;
using Squadtalk.Data.LiveKit;
using Squadtalk.Data.LiveKit.Events;
using Squadtalk.Hubs;

namespace Squadtalk.Services;

public class VoiceCallManager(ILogger<VoiceCallManager> logger)
{
    public IEnumerable<Room> ActiveRooms => _rooms.Values;

    private readonly ConcurrentDictionary<string, UserId> _roomsToInitiate = [];
    private readonly ConcurrentDictionary<string, Room> _rooms = [];
    private readonly ConcurrentDictionary<string, Room> _userToRoomMap = [];

    public Task HandleEventAsync(LiveKitEvent liveKitEvent, IHubContext<ChatHub, IChatClient> hubContext)
    {
        switch (liveKitEvent)
        {
            case RoomStartedEvent roomStartedEvent:
                return OnRoomStarted(roomStartedEvent, hubContext);

            case RoomFinishedEvent roomFinishedEvent:
                return OnRoomFinished(roomFinishedEvent, hubContext);

            case ParticipantJoinedEvent participantJoinedEvent:
                return OnParticipantJoined(participantJoinedEvent);

            case ParticipantLeftEvent participantLeftEvent:
                return OnParticipantLeft(participantLeftEvent);

            default:
                logger.LogInformation("LiveKit event: {Event}", liveKitEvent.EventName);
                return Task.CompletedTask;
        }
    }

    public bool RoomExists(ChannelId channelId)
    {
        return _rooms.ContainsKey(channelId);
    }

    public void VoiceCallInitiated(IChatUser initiator, string channelId)
    {
        _roomsToInitiate.TryAdd(channelId, initiator.Id);
    }

    private async Task OnRoomStarted(RoomStartedEvent roomStartedEvent, IHubContext<ChatHub, IChatClient> hubContext)
    {
        var dto = roomStartedEvent.Room;
        var room = new Room(dto.Id);

        _rooms.TryAdd(dto.Id, room);
        logger.LogInformation("Room {Room} started", room.Id);

        _roomsToInitiate.TryRemove(room.Id, out var initiatorId);

        await hubContext.Clients.Group(room.Id).IncomingCall(new ChannelId(room.Id), initiatorId);
    }

    private async Task OnRoomFinished(RoomFinishedEvent roomFinishedEvent, IHubContext<ChatHub, IChatClient> hubContext)
    {
        var dto = roomFinishedEvent.Room;

        _rooms.TryRemove(dto.Id, out var room);
        room?.Dispose();

        logger.LogInformation("Room {Room} finished", dto.Id);
        _roomsToInitiate.TryRemove(dto.Id, out _);

        await hubContext.Clients.Group(dto.Id).CallEnded(new ChannelId(dto.Id));
    }

    private async Task OnParticipantJoined(ParticipantJoinedEvent participantJoinedEvent)
    {
        var participantDto = participantJoinedEvent.Participant;
        var roomDto = participantJoinedEvent.Room;

        if (!_rooms.TryGetValue(roomDto.Id, out var room))
        {
            logger.LogError("Missing room");
            return;
        }

        _userToRoomMap.TryAdd(participantDto.Sid, room);

        var participant = new Participant(participantDto.Sid, participantDto.Username, participantDto.Id);
        await room.AddParticipantAsync(participant);

        logger.LogInformation("Participant {Participant} joined room {Room}", participant.Name, room.Id);
    }

    private async Task OnParticipantLeft(ParticipantLeftEvent participantLeftEvent)
    {
        var participantDto = participantLeftEvent.Participant;
        var roomDto = participantLeftEvent.Room;

        if (!_rooms.TryGetValue(roomDto.Id, out var room))
        {
            logger.LogError("Missing room");
            return;
        }

        _userToRoomMap.TryRemove(participantDto.Sid, out _);
        await room.RemoveParticipantBySidAsync(participantDto.Sid);

        logger.LogInformation("Participant {Participant} left room {Room}", participantDto.Username, room.Id);
    }
}
