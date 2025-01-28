using Shared.Enums;

namespace Shared.Data;

public interface IGroupParticipant : IChatUser
{
    GroupRole Role { get; }

    IChatUser? AddedBy { get; }
}
