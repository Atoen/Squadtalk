using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class GroupChatModel(IEnumerable<UserModel> others, ChannelId id, string? customName = null) : ChannelModel(id)
{
    public const string GlobalChanelIdValue = "global";

    public static readonly ChannelId GlobalChatId = new(GlobalChanelIdValue);
    public static GroupChatModel CreateGlobalChat() => new([], GlobalChatId) { _name = "Global" };
    
    public override List<UserModel> Others { get; } = others.ToList();
    
    private string? _name;
    public override string Name => CustomName ?? GetOrPrepareName();

    public string? CustomName { get; set; } = customName;

    public override UserStatus Status => GetStatus();

    public bool HasOnlineStatus => Status == UserStatus.Online;

    private string GetOrPrepareName()
    {
        if (_name is not null)
        {
            return _name;
        }

        _name = Others.Count == 0
            ? string.Empty // Group with only 1 user has localized default name
            : string.Join(", ", Others.Select(x => x.Username));

        return _name;
    }

    private UserStatus GetStatus()
    {
        var hasOnlineUser = Others.Any(x => x.Status == UserStatus.Online);
        return hasOnlineUser ? UserStatus.Online : UserStatus.Offline;
    }
}
