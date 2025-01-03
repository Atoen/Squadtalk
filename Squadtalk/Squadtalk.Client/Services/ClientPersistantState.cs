using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Shared;
using Shared.DTOs.Chat;

namespace Squadtalk.Client.Services;

internal class ClientPersistantState(PersistentComponentState persistentComponentState)
{
    public bool TryReadChannels([NotNullWhen(true)] out List<ChannelDto>? channels)
    {
        var read = persistentComponentState.TryTakeFromJson(PersistentStateKeys.Channels, out channels);
        return read && channels is not null;
    }

    public bool TryReadFriends([NotNullWhen(true)] out List<UserDto>? users)
    {
        var read = persistentComponentState.TryTakeFromJson(PersistentStateKeys.Friends, out users);
        return read && users is not null;
    }

    public bool TryReadFriendRequests([NotNullWhen(true)] out List<PendingFriendRequestDto>? friendRequests)
    {
        var read = persistentComponentState.TryTakeFromJson(PersistentStateKeys.FriendRequests, out friendRequests);
        return read && friendRequests is not null;
    }
}
