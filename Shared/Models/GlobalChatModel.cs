using Shared.Enums;

namespace Shared.Models;

public class GlobalChatModel : ChatModel
{
    public override string Name => "Global";

    public override UserStatus Status => UserStatus.Unknown;

    public override IEnumerable<GroupParticipantModel> Participants { get; } = [];

    public override IEnumerable<GroupParticipantModel> Others { get; } = [];

    public override GroupParticipantModel LocalUser { get; }

    public GlobalChatModel(UserModel localUser) : base(GlobalChatId)
    {
        LocalUser = new GroupParticipantModel
        {
            User = localUser
        };
    }
}
