namespace Squadtalk.Data.Entities;

public class Friendship
{
    public int Id { get; set; }

    public ChatUser User1 { get; set; } = default!;
    public ChatUser User2 { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}
