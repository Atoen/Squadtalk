using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Communication;

public class DirectMessageChannelModel(UserModel other, ChannelId id) : TextChannelModel(id)
{
    public const string FakeChannelIdValue = "fake";
    public static readonly ChannelId FakeChannelId = new(FakeChannelIdValue);

    public static DirectMessageChannelModel CreateFakeChannel(UserModel other) => new(other, FakeChannelId);
    
    public UserModel Other { get; } = other;

    public override string Name => Other.Username;
}