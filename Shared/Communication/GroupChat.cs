using Shared.Models;

namespace Shared.Communication;

public class GroupChat(string id) : TextChannel(id)
{
    public const string GlobalChatId = "global";

    public static readonly GroupChat GlobalChat = new(GlobalChatId) { _name = "Global", Others = [] };

    private string? _name;

    public required List<UserModel> Others { get; init; }

    public override string Name => _name ??= string.Join(", ", Others.Select(x => x.Username));
}