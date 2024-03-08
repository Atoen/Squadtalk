using Shared.Models;

namespace Shared.Communication;

public class GroupChat(string id, IEnumerable<UserModel> others) : TextChannel(id)
{
    public const string GlobalChatId = "global";

    public static readonly GroupChat GlobalChat = new(GlobalChatId, []) { _name = "Global"};

    private string? _name;

    public List<UserModel> Others { get; } = others.ToList();

    public override string Name => _name ??= string.Join(", ", Others.Select(x => x.Username));
}