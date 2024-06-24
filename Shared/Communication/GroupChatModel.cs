using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Communication;

public class GroupChatModel(IEnumerable<UserModel> others, ChannelId Id) : TextChannelModel(Id)
{
    public const string GlobalChanelIdValue = "global";
    
    public static readonly ChannelId GlobalChatId = new(GlobalChanelIdValue);
    public static GroupChatModel CreateGlobalChat() => new([], GlobalChatId) { _name = "Global" };
    
    public List<UserModel> Others { get; } = others.ToList();
    
    private string? _name;
    public override string Name => _name ??= string.Join(", ", Others.Select(x => x.Username));
}