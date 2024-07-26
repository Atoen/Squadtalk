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
    public override string Name => CustomName ?? (_name ??= string.Join(", ", Others.Select(x => x.Username)));

    public string? CustomName { get; set; } = customName;

    public override UserStatus Status => GetStatus();

    private UserStatus GetStatus()
    {
        var status = UserStatus.Offline;

        foreach (var user in Others)
        {
            if (user.Status == UserStatus.Online)
            {
                return UserStatus.Online;
            }
            if (user.Status == UserStatus.Away)
            {
                status = UserStatus.Away;
            }
        }

        return status;
    }
}
