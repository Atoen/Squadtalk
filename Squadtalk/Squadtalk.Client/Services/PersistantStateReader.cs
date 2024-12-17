using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Shared.DTOs;

namespace Squadtalk.Client.Services;

public class ClientPersistantState(PersistentComponentState persistentComponentState)
{
    private const string ChannelsKey = "channels";
    private const string UsersKey = "users";

    public bool TryReadChannels([NotNullWhen(true)] out List<ChannelDto>? channels)
    {
        var read = persistentComponentState.TryTakeFromJson(ChannelsKey, out channels);

        return read && channels is not null;
    }

    public bool TryReadUsers([NotNullWhen(true)] out List<UserDto>? users)
    {
        var read = persistentComponentState.TryTakeFromJson(UsersKey, out users);

        return read && users is not null;
    }
}
