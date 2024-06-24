using Shared.Data;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.Entities;

public class Message : IChatMessage
{
    public uint Id { get; set; }

    public ApplicationUser Author { get; set; } = default!;
    
    public ChannelId ChannelId { get; set; } = default!;
    
    public DateTimeOffset Timestamp { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    public Embed? Embed { get; set; }
    
    IChatUser IChatMessage.Author => Author;
    
    IMessageEmbed? IChatMessage.Embed => Embed;
}