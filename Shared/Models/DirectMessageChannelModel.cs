using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class DirectMessageChannelModel(UserModel other, ChannelId id) : ChannelModel(id)
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

    public UserModel Other { get; } = other;

    public override string Name => Other.Username;

    public override IEnumerable<UserModel> Others { get; } = [other];

    public override UserStatus Status => Other.Status;
}
