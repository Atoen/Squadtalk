using Shared.Data.TypedIds;
using Shared.Models;
using Squadtalk.Data.Entities;

namespace Squadtalk.Extensions;

internal static class ChatUserExtensions
{
    public static bool ParticipatesInChannel(this ChatUser user, GroupId groupId)
    {
        if (groupId == GroupChatModel.GlobalChatId)
        {
            return true;
        }

        return user.GroupParticipants is { Count: > 0 } && user.Groups.Any(x => x.Id == groupId);
    }
}
