using System.ComponentModel.DataAnnotations;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Services;

namespace Squadtalk.Data.Entities;

public class Message : IChatMessage
{
    public uint Id { get; set; }

    public ChatUser Author { get; set; } = default!;

    public GroupId GroupId { get; set; } = default!;

    public DateTimeOffset Timestamp { get; set; }

    [StringLength(IFormValidator.MaximumTextMessageLength)]
    public string Content { get; set; } = default!;

    public Embed? Embed { get; set; }

    IChatUser IChatMessage.Author => Author;

    IMessageEmbed? IChatMessage.Embed => Embed;
}