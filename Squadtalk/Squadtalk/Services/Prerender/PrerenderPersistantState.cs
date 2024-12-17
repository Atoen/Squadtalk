using Microsoft.AspNetCore.Components;
using Shared.DTOs;

namespace Squadtalk.Services.Prerender;

public class PrerenderPersistantState(PersistentComponentState persistentComponentState)
{
    private const string ChannelsKey = "channels";
    private const string UsersKey = "users";

    public event Action? DataStored;

    public List<ChannelDto>? Channels { get; private set; }
    public List<UserDto>? Users { get; private set; }

    public void AddData(List<ChannelDto>? channels, List<UserDto>? users)
    {
        Channels = channels;
        Users = users;

        if (channels is not null || users is not null)
        {
            DataStored?.Invoke();
        }
    }

    public void PersistData()
    {
        if (Channels is { Count: > 0 })
        {
            persistentComponentState.PersistAsJson(ChannelsKey, Channels);
        }

        if (Users is { Count: > 0 })
        {
            persistentComponentState.PersistAsJson(UsersKey, Users);
        }
    }
}
