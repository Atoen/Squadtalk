using Shared.Data.TypedIds;

namespace Squadtalk.Data.LiveKit;

public sealed class Room(GroupId groupId) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Dictionary<string, Participant> _participants = [];
    private readonly DateTime _startTime = DateTime.Now;

    public GroupId GroupId { get; } = groupId;
    public UserId InitiatorId { get; private set; }
    public IEnumerable<Participant> Participants => _participants.Values;

    public bool Active { get; private set; } = true;
    public bool CallMissed { get; private set; }
    public TimeSpan Duration { get; private set; }

    private int _maxParticipants;

    public async Task AddParticipantAsync(Participant participant)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_participants.Count == 0 && InitiatorId == default)
            {
                InitiatorId = participant.Id;
            }

            _participants.Add(participant.Sid, participant);
            if (_participants.Count > _maxParticipants)
            {
                _maxParticipants = _participants.Count;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RemoveParticipantBySidAsync(string sid)
    {
        await _semaphore.WaitAsync();

        try
        {
            _participants.Remove(sid);
            if (_participants.Count == 0)
            {
                Active = false;
                Duration = DateTime.Now - _startTime;
                CallMissed = _maxParticipants <= 1;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose() => _semaphore.Dispose();
}
