using System.ComponentModel.DataAnnotations.Schema;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.Entities;

public class Friendship
{
    public int Id { get; set; }

    [ForeignKey(nameof(User1))]
    public UserId User1Id { get; set; }
    public ChatUser User1 { get; set; } = default!;

    [ForeignKey(nameof(User2))]
    public UserId User2Id { get; set; }
    public ChatUser User2 { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}
