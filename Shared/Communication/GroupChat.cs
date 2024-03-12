using Shared.Data;
using Shared.Models;

namespace Shared.Communication;

public class GroupChat(IEnumerable<UserModel> others, ChannelId id) : TextChannel(id)
{
    public const string GlobalChanelIdValue = "global";
    public static readonly ChannelId GlobalChatId = new(GlobalChanelIdValue);

    public static readonly GroupChat GlobalChat = new([], GlobalChatId) { _name = "Global"};

    public List<UserModel> Others { get; } = others.ToList();
    
    private string? _name;
    public override string Name => _name ??= string.Join(", ", Others.Select(x => x.Username));
}