using System.Security.Claims;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Squadtalk.Data.Entities;

namespace Squadtalk.Extensions;

internal static class ChannelExtensions
{
    public static Group WithLastMessage(this Group textGroup, Message message)
    {
        textGroup.LastMessage = message;
        return textGroup;
    }

    public static bool UserParticipatesInGroup(this Group group, ClaimsPrincipal? principal)
    {
        if (principal?.GetClaimValue(ClaimTypes.NameIdentifier) is not { } claim)
        {
            return false;
        }

        return UserId.TryParse(claim, out var userId) && group.UserParticipatesInGroupAsync(userId);
    }

    public static bool UserParticipatesInGroupAsync(this Group group, UserId userId)
    {
        return group.Participants.Any(x => x.UserId == userId);
    }
}
