using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Squadtalk.Data;

namespace Squadtalk.Hubs;

[Authorize]
public partial class ChatHub
{
    public bool MeasurePing() => true;

    public async Task<CallOfferId?> StartCall(ChannelId channelId)
    {
        if (await _userManager.GetUserAsync(Context.User!) is not { } callingUser)
        {
            await VoiceCaller.CallFailed("Failed to create voice call");
            return null;
        }

        var callChannel = await _dbContext.Channels
            .AsNoTracking()
            .Include(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == channelId);

        if (callChannel is not {Participants: { Count: > 1 } callParticipants})
        {
            await VoiceCaller.CallFailed("Unable start the call");
            return null;
        }

        var usersToNotify = callParticipants.Where(x => x.Id != callingUser.Id)
            .ToImmutableList();

        var caller = new VoiceUser(callingUser, (SignalRConnectionId) Context.ConnectionId);
        var offer = new VoiceCallOffer(caller, usersToNotify, channelId, CallOfferId.New);
        _voiceCallManager.AddCallOffer(offer);

        var callerDto = callingUser.ToDto();

        var connections = usersToNotify
            .SelectMany(x => _connectionManager.GetUserConnections(x));

        foreach (var connection in connections)
        {
            await VoiceClient(connection).IncomingCall(callerDto, offer.Id);
        }

        return offer.Id;
    }

    public async Task AcceptCall(CallOfferId id)
    {
        if (_voiceCallManager.GetVoiceCallOffer(id) is not { } offer) return;
        if (await _userManager.GetUserAsync(Context.User!) is not { } acceptingUser) return;

        var callee = new VoiceUser(acceptingUser, (SignalRConnectionId) Context.ConnectionId);
        var call = new VoiceCall { Users = [offer.Caller, callee], Id = CallId.New, ChannelId = offer.ChannelId };

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
    
    public async Task EndCall(CallId id = default)
    {
        var call = id == default
            ? _voiceCallManager.GetCall((SignalRConnectionId) Context.ConnectionId)
            : _voiceCallManager.GetCall(id);
        
        if (call is null) return;
        if (await _userManager.GetUserAsync(Context.User!) is not { } user) return;
        
        _voiceCallManager.RemoveCallOffersFromUser(user);
        foreach (var (_, connectionId) in call.Users.Where(x => x.ConnectionId != Context.ConnectionId))
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
        
        try
        {
            await foreach (var packet in stream)
            {
                if (call.Users.Count < 2)
                {
                    _logger.LogInformation("Stream ended");
                    return;
                }

                await OthersInVoiceGroup(call.GroupName).GetVoicePacket(new VoicePacketDto(user.Id, packet));
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stream ended");
        }
    }
}