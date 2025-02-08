using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Reactive;

namespace Shared.Models;

public sealed class DirectMessageModel : ChatModel
{
    public bool IsTemporary { get; init; }

    public UserModel Other { get; }

    public override string Name => Other.Username;

    public override List<GroupParticipantModel> Participants { get; }

    public override IEnumerable<GroupParticipantModel> Others { get; }

    public override GroupParticipantModel LocalUser { get; }

    public override UserStatus Status => Other.Status;

    public DirectMessageModel(IEnumerable<GroupParticipantModel> participants, GroupId id) : base(id)
    {
        Participants = participants.ToList();
        LocalUser = Participants.Single(x => x.User.IsLocal);

        var otherParticipant = Participants.Single(x => x.User.IsRemote);
        Others = [otherParticipant];
        Other = otherParticipant.User;
    }

    public static DirectMessageModel CreateTempChannel(UserModel localUser, UserModel other)
    {
        var id = GroupId.From(other.Username);

        var localParticipant = new GroupParticipantModel { User = localUser };
        var tempParticipant = new GroupParticipantModel { User = other };

        return new DirectMessageModel([localParticipant, tempParticipant], id)
        {
            IsTemporary = true
        };
    }

    public override IDisposable? Subscribe(ISubscriber subscriber) => Other.Subscribe(subscriber);
}
