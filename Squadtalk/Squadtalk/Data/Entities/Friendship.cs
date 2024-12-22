namespace Squadtalk.Data.Entities;

public class Friendship
{
    public int Id { get; set; }

    public ApplicationUser User1 { get; set; } = default!;
    public ApplicationUser User2 { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}
