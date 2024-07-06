namespace Squadtalk.Data.LiveKit;

public sealed class Room(string id) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1);

    public string Id { get; } = id;

    public IEnumerable<Participant> Participants => _participants;

    private readonly List<Participant> _participants = [];

    public async Task AddParticipantAsync(Participant participant)
    {
        await _semaphore.WaitAsync();

        try
        {
            _participants.Add(participant);
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
            _participants.RemoveAll(x => x.Sid == sid);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose() => _semaphore.Dispose();
}
