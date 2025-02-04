using System.Diagnostics.CodeAnalysis;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class MessageModel
{
    public MessageId Id { get; set; }

    public UserModel Author { get; set; } = default!;

    public string Content { get; set; } = default!;

    public DateTimeOffset Timestamp { get; set; }

    public bool IsSeparate { get; set; }

    public EmbedModel? Embed { get; set; }

    [MemberNotNullWhen(true, nameof(Embed))]
    public bool IsSystemMessage => Embed is { Type: EmbedType.SystemMessage };
}
