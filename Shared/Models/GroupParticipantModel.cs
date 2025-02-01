using System.Diagnostics.CodeAnalysis;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class GroupParticipantModel : IGroupParticipant
{
    public required UserModel User { get; init; }

    public GroupRole Role { get; set; }

    public UserModel? AddedBy { get; init; }

    [MemberNotNullWhen(false, nameof(AddedBy))]
    public bool IsGroupCreator => Role == GroupRole.Owner;

    public static GroupParticipantModel Create(IGroupParticipant groupParticipant, Func<IChatUser, UserModel> userModelProvider)
    {
        return new GroupParticipantModel
        {
            User = userModelProvider(groupParticipant),
            Role = groupParticipant.Role,
            AddedBy = groupParticipant.AddedBy is { } addedBy ? userModelProvider(addedBy) : null
        };
    }

    IChatUser? IGroupParticipant.AddedBy => AddedBy;
    string IChatUser.Username => User.Username;
    UserId IChatUser.Id => User.Id;
    UserStatus IChatUser.Status => User.Status;
}

