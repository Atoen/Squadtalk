using Squadtalk.Shared;

namespace Squadtalk.Client.Models.Communication;

public sealed class GroupChat : Channel
{
    public static readonly Guid GlobalChatId = Guid.Empty;
    
    public static readonly GroupChat GlobalChat = new(GlobalChatId)
    {
        _name = "Global",
        Others = new List<UserDto>(),
        IsSelected = true
    };

    private string? _name;

    public required List<UserDto> Others { get; init; }

    public override string Name => _name ??= string.Join(", ", Others.Select(x => x.Username));

    public GroupChat(Guid id) : base(id)
    {
    }
}