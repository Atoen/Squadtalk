using System.ComponentModel.DataAnnotations.Schema;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.Entities;

public class FriendRequest
{
    public FriendRequestId Id { get; set; }

    [ForeignKey(nameof(Requester))]
    public UserId RequesterId { get; set; }
    public ChatUser Requester { get; set; } = default!;

    [ForeignKey(nameof(Recipient))]
    public UserId RecipientId { get; set; }
    public ChatUser Recipient { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }

    public bool? IsAccepted { get; set; }
}
