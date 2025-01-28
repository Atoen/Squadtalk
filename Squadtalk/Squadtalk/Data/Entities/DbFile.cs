using Shared.Data.TypedIds;
using Squadtalk.Data.TypedIds;

namespace Squadtalk.Data.Entities;

public class DbFile
{
    public uint Id { get; set; }

    public TusFileId TusId { get; set; } = default!;

    public GroupId GroupId { get; set; } = default!;
}
