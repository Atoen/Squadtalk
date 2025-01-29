using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Reactive;

namespace Shared.Models;

public sealed class DirectMessageModel : ChatModel, ISubscriber<UserModel>, IDisposable
{
    public bool IsTemporary { get; init; }

    public GroupParticipantModel Other { get; }

    public override string Name => Other.User.Username;

    public override List<GroupParticipantModel> Participants { get; }

    public override IEnumerable<GroupParticipantModel> Others { get; }

    public override GroupParticipantModel LocalUser { get; }

    public override UserStatus Status => Other.User.Status;

    private readonly IDisposable? _subscription;

    public DirectMessageModel(IEnumerable<GroupParticipantModel> participants, GroupId id) : base(id)
    {
        Participants = participants.ToList();
        LocalUser = Participants.Single(x => x.User.IsLocal);
        Other = Participants.Single(x => x.User.IsRemote);
        Others = [Other];

        _subscription = Other.User.Subscribe(this);
    }

    public static DirectMessageModel CreateTempChannel(UserModel userModel)
    {
        var id = GroupId.From(userModel.Username);
        var participant = new GroupParticipantModel { User = userModel };

        return new DirectMessageModel([participant], id)
        {
            IsTemporary = true
        };
    }

    public void OnNext(UserModel value) => Notify(this);

    public void Dispose() => _subscription?.Dispose();
}
