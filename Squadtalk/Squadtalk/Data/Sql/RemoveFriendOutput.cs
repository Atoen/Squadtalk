using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Squadtalk.Data.Sql;

[Keyless]
public class RemoveFriendOutput
{
    [Column("success")]
    public bool Success { get; set; }
}
