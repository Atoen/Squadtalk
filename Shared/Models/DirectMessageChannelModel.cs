using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Reactive;

namespace Shared.Models;

public class DirectMessageChannelModel : ChannelModel, ISubscriber<UserModel>, IDisposable
{
    public static DirectMessageChannelModel CreateTempChannel(UserModel other)
    {
        var id = ChannelId.From(other.Username);
        return new DirectMessageChannelModel(other, id)
        {
            IsTemporary = true
        };
    }

    public bool IsTemporary { get; init; }

    public UserModel Other { get; }

    public override string Name => Other.Username;

    public override IEnumerable<UserModel> Others { get; }

    public override UserStatus Status => Other.Status;

    private readonly IDisposable? _subscription;

    public DirectMessageChannelModel(UserModel other, ChannelId id) : base(id)
    {
        Other = other;
        Others = [other];

        _subscription = Other.Subscribe(this);
    }

    public void OnNext(UserModel value) => Notify(this);

    public void Dispose() => _subscription?.Dispose();
}
