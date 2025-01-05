using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Squadtalk.Data.Sql;

[Keyless]
public class FriendRequestResponseOutput
{
    [Column("status_code")]
    public FriendRequestResponseResult Status { get; set; }

    [Column("friendship_id")]
    public int? AddedFriendshipId { get; set; }

    [Column("requester_id")]
    public Guid? RequesterId { get; set; }

    [Column("other_way_request_id")]
    public int? OtherWayRequestId { get; set; }
}
