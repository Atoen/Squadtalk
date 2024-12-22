namespace Squadtalk.Data.Entities;

public class FriendRequest
{
    public int Id { get; set; }

    public ApplicationUser Requester { get; set; } = default!;
    public ApplicationUser Recipient { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }

    public bool? IsAccepted { get; set; }
}