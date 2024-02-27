using Shared.Models;

namespace Shared.Communication;

public class DirectMessageChannel(UserModel other, string id) : TextChannel(id)
{
    public const string FakeChannelId = "fake";

    public static DirectMessageChannel CreateFakeChannel(UserModel other) => new(other, FakeChannelId);
    
    public UserModel Other { get; } = other;

    public override string Name => Other.Username;

    public override bool Selected
    {
        get => Other.Selected;
        set => Other.Selected = value;
    }
}