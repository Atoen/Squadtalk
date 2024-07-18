using System.Collections.Concurrent;
using Shared.Data;
using Shared.Data.TypedIds;
using Squadtalk.Data.LiveKit;
using Squadtalk.Data.LiveKit.DTOs;

namespace Squadtalk.Services;

public class VoiceCallManager(ILogger<VoiceCallManager> logger)
{
    public IEnumerable<Room> ActiveRooms => _rooms.Values;

    private readonly ConcurrentDictionary<ChannelId, UserId> _roomsToInitiate = [];
    private readonly ConcurrentDictionary<ChannelId, Room> _rooms = [];

    public bool ChannelHasActiveCall(ChannelId channelId)
    {
        return _rooms.TryGetValue(channelId, out var room) && room.Active;
    }

    public void VoiceCallInitiated(IChatUser initiator, ChannelId channelId)
    {
        _roomsToInitiate.TryAdd(channelId, initiator.Id);
    }

    public UserId AddRoom(RoomDto dto)
    {
        var room = new Room(dto.ChannelId);
        _rooms.TryAdd(room.ChannelId, room);

        _roomsToInitiate.TryRemove(room.ChannelId, out var initiatorId);

        return initiatorId;
    }

    public async Task AddParticipantAsync(ParticipantDto participantDto, ChannelId channelId)
    {
        if (!_rooms.TryGetValue(channelId, out var room))
        {
            logger.LogError("Missing room");
            return;
        }

        var participant = new Participant(participantDto.Sid, participantDto.Username, participantDto.Id);
        await room.AddParticipantAsync(participant);
    }

    public async Task<bool> RemoveParticipantAsync(ParticipantDto participantDto, ChannelId channelId)
    {
        if (!_rooms.TryGetValue(channelId, out var room))
        {
            logger.LogError("Missing room");
            return false;
        }

        await room.RemoveParticipantBySidAsync(participantDto.Sid);
        return !room.Active;
    }

    public bool RemoveRoom(ChannelId channelId, out (UserId initiatorId, TimeSpan callDuration, bool callMissed) tuple)
    {
        tuple = default;

        _roomsToInitiate.TryRemove(channelId, out _);
        if (!_rooms.TryRemove(channelId, out var room))
        {
            return false;
        }

        tuple = (room.InitiatorId, room.Duration, room.CallMissed);

        room.Dispose();

        return true;
    }
}
