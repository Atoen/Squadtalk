using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.DTOs;
using Squadtalk.Data;

namespace Squadtalk.Hubs;

[Authorize]
public partial class ChatHub
{
    public bool MeasurePing() => true;

    public async Task<CallOfferId?> StartCall(UserId id)
    {
        if (await _userManager.GetUserAsync(Context.User!) is not { } callingUser)
        {
            await VoiceCaller.CallFailed("Failed to create voice call");
            return null;
        }

        var targetUser = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id.Value);

        if (targetUser is null)
        {
            await VoiceCaller.CallFailed("Unable to call the user");
            return null;
        }

        var caller = new VoiceUser(callingUser, (SignalrConnectionId) Context.ConnectionId);
        var offer = new VoiceCallOffer(caller, targetUser, CallOfferId.New);
        _voiceCallManager.AddCallOffer(offer);

        var dto = callingUser.ToDto();
        foreach (var connection in _connectionManager.GetUserConnections(targetUser))
        {
            await VoiceClient(connection).IncomingCall(dto, offer.Id);
        }

        return offer.Id;
    }

    public async Task AcceptCall(CallOfferId id)
    {
        if (_voiceCallManager.GetVoiceCallOffer(id) is not { } offer) return;
        if (await _userManager.GetUserAsync(Context.User!) is not { } acceptingUser) return;

        var callee = new VoiceUser(acceptingUser, (SignalrConnectionId) Context.ConnectionId);
        var call = new VoiceCall { Users = [offer.Caller, callee], Id = new CallId(id.Value) };

        _voiceCallManager.RemoveCallOffer(offer.Id);
        _voiceCallManager.AddCall(call);

        await VoiceClient(offer.Caller.ConnectionId).CallAccepted(id);

        var callUsers = call.Users.Select(x => x.User.ToDto()).ToList();
        foreach (var (_, connectionId) in call.Users)
        {
            await VoiceClient(connectionId).GetCallUsers(callUsers, call.Id);
            await Groups.AddToGroupAsync(connectionId, call.GroupName);
        }
    }

    public async Task DeclineCall(CallOfferId id)
    {
        if (_voiceCallManager.GetVoiceCallOffer(id) is not { } offer) return;
        if (await _userManager.GetUserAsync(Context.User!) is null) return;

        _voiceCallManager.RemoveCallOffer(offer.Id);

        await VoiceClient(offer.Caller.ConnectionId).CallDeclined(id);
    }
    
    public async Task EndCall(CallId? id = null)
    {
        var call = id is null
            ? _voiceCallManager.GetCall((SignalrConnectionId) Context.ConnectionId)
            : _voiceCallManager.GetCall(id);
        
        if (call is null) return;
        if (await _userManager.GetUserAsync(Context.User!) is not { } user) return;
        
        _voiceCallManager.RemoveCallOffersFromUser(user);
        foreach (var (_, connectionId) in call.Users.Where(x => x.ConnectionId.Value != Context.ConnectionId))
        {
            await VoiceClient(connectionId).CallEnded(call.Id);
            await Groups.RemoveFromGroupAsync(connectionId, call.GroupName);
        }
        
        _voiceCallManager.RemoveCall(call);
    }

    public async Task StartStream(CallId id, IAsyncEnumerable<byte[]> stream)
    {
        if (_voiceCallManager.GetCall(id) is not { } call) return;
        if (await _userManager.GetUserAsync(Context.User!) is not { } user) return;

        _logger.LogInformation("Stream started from {User}", user.UserName);

        var userId = new UserId(user.Id);

        try
        {
            await foreach (var packet in stream)
            {
                if (call.Users.Count < 2)
                {
                    _logger.LogInformation("Stream ended");
                    return;
                }

                await OtherInVoiceGroup(call.GroupName)
                    .GetVoicePacket(new VoicePacketDto(userId, packet));
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stream ended");
        }
    }
}