using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Squadtalk.Data;

namespace Squadtalk.Hubs;

[Authorize]
public partial class ChatHub
{
    public bool MeasurePing() => true;

    public async Task StartCall(List<string> invitedIds)
    {
        var result = await CreateVoiceCall(invitedIds);
        if (result is not { voiceCall: { } call })
        {
            _logger.LogError("Error: {Error}", result.error);
            await VoiceCaller.CallFailed(result.error);
            return;
        }

        await _voiceCallManager.AddCallAsync(call);

        var invitedUsersConnectionIds = _voiceCallManager.GetInvitedUsersConnectionIds(call).ToList();
        
        _logger.LogInformation("Sending call invitation to {User} ({Connections} connections)", call.Invited[0], 
            invitedUsersConnectionIds.Count);

        foreach (var connectionId in invitedUsersConnectionIds)
        {
            await VoiceClient(connectionId).CallOfferIncoming(call.ToDto());
        }   
    }

    public async Task AcceptCall(string callId)
    {
        var user = await _userManager.GetUserAsync(Context.User!);
        if (user is null)
        {
            _logger.LogError("małe oro");
            return;
        }

        var call = _voiceCallManager.GetVoiceCall(callId);
        if (call is null) return;

        var connectionsInCall = _voiceCallManager.GetConnectionsInVoiceCall(call);
        
        var added = await _voiceCallManager.AddUserToCallAsync(user, callId);
        if (!added) return;
        
        foreach (var connection in connectionsInCall)
        {
            await VoiceClient(connection).UserJoinedCall(user.ToDto(), callId);
        }
    }
    
    private async Task<(VoiceCall? voiceCall, string? error)> CreateVoiceCall(List<string> invitedIds)
    {
        static (VoiceCall?, string?) Error(string message) => (null, message);
        static (VoiceCall?, string?) Success(VoiceCall voiceCall) => (voiceCall, null);

        var invitedCount = invitedIds.Count;
        if (invitedCount < 1 || invitedIds.Distinct().Count() != invitedCount)
        {
            return Error("Invalid list of invited users");
        }

        var invited = await _dbContext.Users
            .Where(x => invitedIds.Contains(x.Id))
            .ToListAsync();

        if (invited.Count != invitedCount)
        {
            return Error("Failed to create call");
        }

        var initiator = await _userManager.GetUserAsync(Context.User!);
        if (initiator is null)
        {
            return Error("Caller is not present in the call");
        }

        var call = new VoiceCall
        {
            Id = Guid.NewGuid().ToString(),
            Initiator = initiator,
            Invited = invited,
            ConnectedUsers = [new VoiceCall.Participant(initiator, Context.ConnectionId)]
        };

        return Success(call);
    }
}