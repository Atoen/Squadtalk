using Shared.Enums;

namespace Shared.Data;

public static class GroupRolePermissions
{
    public static bool CanAddNewMembers(this IGroupParticipant participant) => true;

    public static bool CanChangeGroupNameAndImage(this IGroupParticipant participant) => participant.Role >= GroupRole.Moderator;

    public static bool CanKick(this IGroupParticipant participant, IGroupParticipant other) => participant.Role > other.Role;

    public static bool CanManageRole(this IGroupParticipant participant, IGroupParticipant other) =>
        participant.Role > GroupRole.Moderator && participant.Role > other.Role;

    public static bool IsModeratorOrAbove(this IGroupParticipant participant) => participant.Role >= GroupRole.Moderator;

    public static bool CanAddModerators(this IGroupParticipant participant) => participant.Role >= GroupRole.Administrator;

    public static bool CanAddAdministrators(this IGroupParticipant participant) => participant.Role == GroupRole.Owner;

    public static bool CanDeleteGroup(this IGroupParticipant participant) => participant.Role == GroupRole.Owner;

    public static bool CanChangeRoleTo(this IGroupParticipant participant, IGroupParticipant other, GroupRole targetRole)
    {
        if (participant.Role is GroupRole.Member or GroupRole.Moderator)
        {
            return false;
        }

        if (other.Role == targetRole)
        {
            return false;
        }

        return (participant.Role, targetRole) switch
        {
            (GroupRole.Administrator, <= GroupRole.Moderator) => true,
            (GroupRole.Owner, <= GroupRole.Administrator) => true,
            _ => false
        };
    }

    // public static bool CanPromoteTo(this IGroupParticipant participant, GroupRole newRole, IGroupParticipant other) =>
    //     CanPromoteTo(participant.Role, newRole, other.Role);
    //
    // public static bool CanPromoteTo(this GroupRole role, GroupRole newRole, GroupRole currentRole)
    // {
    //     if (newRole <= currentRole)
    //     {
    //         return false;
    //     }
    //
    //     return (role, newRole) switch
    //     {
    //         (GroupRole.Owner, <= GroupRole.Administrator) => true,
    //         (GroupRole.Administrator, <= GroupRole.Moderator) => true,
    //         _ => false
    //     };
    // }
    //
    // public static bool CanDemoteTo(this IGroupParticipant participant, GroupRole newRole, IGroupParticipant other) =>
    //     CanDemoteTo(participant.Role, newRole, other.Role);
    //
    // public static bool CanDemoteTo(this GroupRole role, GroupRole newRole, GroupRole currentRole)
    // {
    //     if (newRole >= currentRole)
    //     {
    //         return false;
    //     }
    //
    //     return (role, newRole) switch
    //     {
    //         (GroupRole.Owner, <= GroupRole.Administrator) => true,
    //         (GroupRole.Administrator, <= GroupRole.Moderator) => true,
    //         _ => false
    //     };
    // }
}
