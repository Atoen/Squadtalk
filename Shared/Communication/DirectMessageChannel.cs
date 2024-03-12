using Shared.Data;
using Shared.Models;

namespace Shared.Communication;

public class DirectMessageChannel(UserModel other, ChannelId id) : TextChannel(id)
{
    public const string FakeChannelIdValue = "fake";
    public static readonly ChannelId FakeChannelId = new(FakeChannelIdValue);

    public static DirectMessageChannel CreateFakeChannel(UserModel other) => new(other, FakeChannelId);
    
    public UserModel Other { get; } = other;

    public override string Name => Other.Username;
}