using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Models;

namespace Shared.Extensions;

public static class GroupParticipantModelExtensions
{
    public static string Username(this GroupParticipantModel participant) => participant.User.Username;

    public static string AvatarUrl(this GroupParticipantModel participant) => participant.User.AvatarUrl;

    public static UserId Id(this GroupParticipantModel participant) => participant.User.Id;

    public static UserStatus Status(this GroupParticipantModel participant) => participant.User.Status;

    public static bool IsLocal(this GroupParticipantModel participant) => participant.User.IsLocal;

    public static bool IsRemote(this GroupParticipantModel participant) => participant.User.IsRemote;
}
