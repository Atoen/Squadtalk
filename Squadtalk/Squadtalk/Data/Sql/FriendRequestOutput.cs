using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Squadtalk.Data.Sql;

[Keyless]
public class FriendRequestOutput
{
    [Column("status_code")]
    public FriendRequestResult Status { get; set; }

    [Column("friend_request_id")]
    public int? AddedRequestId { get; set; }
}
