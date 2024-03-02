using Squadtalk.Data;

namespace Squadtalk.Services;

public class VoiceCallManager
{
    private readonly ChatConnectionManager<ApplicationUser, string> _connectionManager;
    private readonly Dictionary<string, VoiceCall> _activeCalls = [];
    private readonly SemaphoreSlim _semaphore = new(1);

    public VoiceCallManager(ChatConnectionManager<ApplicationUser, string> connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task AddCallAsync(VoiceCall voiceCall)
    {
        await _semaphore.WaitAsync();
        try
        {
            _activeCalls.Add(voiceCall.Id, voiceCall);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public VoiceCall? GetVoiceCall(string callId) => _activeCalls.GetValueOrDefault(callId);

    public IEnumerable<string> GetInvitedUsersConnectionIds(VoiceCall call)
    {
        return call.Invited.SelectMany(x => _connectionManager.GetUserConnections(x));
    }

    public IEnumerable<string> GetConnectionsInVoiceCall(VoiceCall call)
    {
        return call.ConnectedUsers.Select(x => x.SignalrConnectionId);
    }

    public async Task<bool> AddUserToCallAsync(ApplicationUser user, string callId)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!_activeCalls.TryGetValue(callId, out var call))
            {
                return false;
            }

            if (call.Invited.All(x => x.Id != user.Id))
            {
                return false;
            }
        
            call.AddParticipant(user, callId);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
        
    }

    public async Task RemoveUserFromCall()
    {

    }
}