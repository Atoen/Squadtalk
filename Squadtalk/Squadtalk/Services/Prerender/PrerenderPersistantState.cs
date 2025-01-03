using Microsoft.AspNetCore.Components;
using Shared;
using Shared.DTOs.Chat;

namespace Squadtalk.Services.Prerender;

internal class PrerenderPersistantState(PersistentComponentState persistentComponentState)
{
    public IList<ChannelDto>? Channels { get; private set; }
    public IList<UserDto>? Friends { get; private set; }
    public IList<PendingFriendRequestDto>? FriendRequests { get; private set; }

    public bool ContainsData => Channels is { Count: > 0 } ||
                                Friends is { Count: > 0 } ||
                                FriendRequests is { Count: > 0 };

    public void AddData(List<ChannelDto>? channels, List<UserDto>? friends, List<PendingFriendRequestDto>? friendRequests)
    {
        Channels = channels;
        Friends = friends;
        FriendRequests = friendRequests;
    }

    public void PersistData()
    {
        if (Channels is { Count: > 0 })
        {
            persistentComponentState.PersistAsJson(PersistentStateKeys.Channels, Channels);
        }

        if (Friends is { Count: > 0 })
        {
            persistentComponentState.PersistAsJson(PersistentStateKeys.Friends, Friends);
        }

        if (FriendRequests is { Count: > 0 })
        {
            persistentComponentState.PersistAsJson(PersistentStateKeys.FriendRequests, FriendRequests);
        }
    }
}
