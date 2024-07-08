using Microsoft.AspNetCore.Authorization;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Squadtalk.Data;

namespace Squadtalk.Hubs;

[Authorize]
public partial class ChatHub
{
    public async Task<RoomTokenDto?> StartCall(ChannelId channelId)
    {
        if (await GetUserWithChannelsAsync(Context.User) is not { } callingUser)
        {
            await VoiceCaller.CallFailed("Failed to create voice call");
            return null;
        }

        if (!UserParticipatesInChannel(callingUser, channelId))
        {
            return null;
        }

        _voiceCallManager.VoiceCallInitiated(callingUser, channelId);

        return _liveKitService.CreateRoomToken(Context.User, channelId);
    }

    public async Task<RoomTokenDto?> AcceptCall(ChannelId channelId)
    {
        if (await GetUserWithChannelsAsync(Context.User) is not { } user)
        {
            await VoiceCaller.CallFailed("Failed to join the voice call");
            return null;
        }

        if (!UserParticipatesInChannel(user, channelId))
        {
            return null;
        }

        if (_voiceCallManager.ActiveRooms.All(x => x.Id != channelId))
        {
            return null;
        }

        await OthersInVoiceGroup(channelId).CallAccepted(channelId, user.ToDto());

        return _liveKitService.CreateRoomToken(Context.User, channelId);
    }

    public async Task DeclineCall(ChannelId channelId)
    {
        if (await GetUserWithChannelsAsync(Context.User) is not { } user)
        {
            return;
        }

        var room = _voiceCallManager.ActiveRooms.SingleOrDefault(x => x.Id == channelId);
        if (room is null)
        {
            return;
        }

        await OthersInVoiceGroup(channelId).CallDeclined(user.ToDto(), channelId);
    }

    public async Task<bool> ChannelHasActiveCall(ChannelId channelId)
    {
        if (await GetUserWithChannelsAsync(Context.User) is not { } user)
        {
            return false;
        }

        return UserParticipatesInChannel(user, channelId) && _voiceCallManager.RoomExists(channelId);
    }
}
