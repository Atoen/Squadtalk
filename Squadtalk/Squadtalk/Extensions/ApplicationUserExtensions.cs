using Shared.Data.TypedIds;
using Squadtalk.Data.Entities;

namespace Squadtalk.Extensions;

public static class ApplicationUserExtensions
{
    public static bool ParticipatesInChannel(this ApplicationUser user, ChannelId channelId)
    {
        return user.Channels is { Count: > 0 } channels && channels.Any(x => x.Id == channelId);
    }
}
