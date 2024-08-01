using Shared.Data.TypedIds;
using Shared.Models;
using Squadtalk.Data.Entities;

namespace Squadtalk.Extensions;

public static class ApplicationUserExtensions
{
    public static bool ParticipatesInChannel(this ApplicationUser user, ChannelId channelId)
    {
        if (channelId == GroupChatModel.GlobalChatId)
        {
            return true;
        }

        return user.Channels is { Count: > 0 } channels && channels.Any(x => x.Id == channelId);
    }
}
