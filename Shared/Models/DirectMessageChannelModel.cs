using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class DirectMessageChannelModel(UserModel other, ChannelId id) : ChannelModel(id)
{
    public const string FakeChannelIdValue = "fake";
    public static readonly ChannelId FakeChannelId = new(FakeChannelIdValue);

    public static DirectMessageChannelModel CreateFakeChannel(UserModel other) => new(other, FakeChannelId);
    
    public UserModel Other { get; } = other;

    public override string Name => Other.Username;

    public override IEnumerable<UserModel> Others { get; } = [other];

    public override UserStatus Status => Other.Status;
}
