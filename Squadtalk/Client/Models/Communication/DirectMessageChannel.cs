using Squadtalk.Client.Shared;

namespace Squadtalk.Client.Models.Communication;

public sealed class DirectMessageChannel : Channel
{
    public static readonly Guid FakeChannelId = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    
    public required User Other { get; init; }

    public override string Name => Other.Username;

    public override bool IsSelected
    {
        get => Other.IsSelected;
        set => Other.IsSelected = value;
    }

    public DirectMessageChannel(Guid id) : base(id)
    {
    }

    public static DirectMessageChannel CreateFakeChannel(User otherUser) => new(FakeChannelId)
    {
        Other = otherUser
    };
}