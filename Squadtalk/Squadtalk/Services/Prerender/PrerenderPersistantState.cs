using Microsoft.AspNetCore.Components;
using Shared.DTOs.Chat;

namespace Squadtalk.Services.Prerender;

internal class PrerenderPersistantState(PersistentComponentState persistentComponentState)
{
    private const string ChannelsKey = "channels";
    private const string UsersKey = "users";

    public IList<ChannelDto>? Channels { get; private set; }
    public IList<UserDto>? OnlineUsers { get; private set; }

    public bool ContainsData => OnlineUsers is { Count: > 0 } || Channels is { Count: > 0 };

    public void AddData(List<ChannelDto>? channels, List<UserDto>? users)
    {
        Channels = channels;
        OnlineUsers = users;
    }

    public void PersistData()
    {
        if (Channels is { Count: > 0 })
        {
            persistentComponentState.PersistAsJson(ChannelsKey, Channels);
        }

        if (OnlineUsers is { Count: > 0 })
        {
            persistentComponentState.PersistAsJson(UsersKey, OnlineUsers);
        }
    }
}
