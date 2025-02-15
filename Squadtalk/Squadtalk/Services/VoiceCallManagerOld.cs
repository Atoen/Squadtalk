using System.Collections.Concurrent;
using Shared.Data;
using Shared.Data.TypedIds;
using Squadtalk.Data.LiveKit;
using Squadtalk.Data.LiveKit.DTOs;

namespace Squadtalk.Services;

public class VoiceCallManagerOld(ILogger<VoiceCallManagerOld> logger)
{
    public IEnumerable<Room> ActiveRooms => _rooms.Values;

    private readonly ConcurrentDictionary<GroupId, UserId> _roomsToInitiate = [];
    private readonly ConcurrentDictionary<GroupId, Room> _rooms = [];

    public bool ChannelHasActiveCall(GroupId groupId)
    {
        return _rooms.TryGetValue(groupId, out var room) && room.Active;
    }

    public void VoiceCallInitiated(IChatUser initiator, GroupId groupId)
    {
        _roomsToInitiate.TryAdd(groupId, initiator.Id);
    }

    public UserId AddRoom(RoomDto dto)
    {
        var room = new Room(dto.GroupId);
        _rooms.TryAdd(room.GroupId, room);

        _roomsToInitiate.TryRemove(room.GroupId, out var initiatorId);

        return initiatorId;
    }

    public async Task AddParticipantAsync(ParticipantDto participantDto, GroupId groupId)
    {
        if (!_rooms.TryGetValue(groupId, out var room))
        {
            logger.LogError("Missing room");
            return;
        }

        var participant = new Participant(participantDto.Sid, participantDto.Username, participantDto.Id);
        await room.AddParticipantAsync(participant);
    }

    public async Task<bool> RemoveParticipantAsync(ParticipantDto participantDto, GroupId groupId)
    {
        if (!_rooms.TryGetValue(groupId, out var room))
        {
            logger.LogError("Missing room");
            return false;
        }

        await room.RemoveParticipantBySidAsync(participantDto.Sid);
        return !room.Active;
    }

    public bool RemoveRoom(GroupId groupId, out (UserId initiatorId, TimeSpan callDuration, bool callMissed) tuple)
    {
        tuple = default;

        _roomsToInitiate.TryRemove(groupId, out _);
        if (!_rooms.TryRemove(groupId, out var room))
        {
            return false;
        }

        tuple = (room.InitiatorId, room.Duration, room.CallMissed);

        room.Dispose();

        return true;
    }
}
