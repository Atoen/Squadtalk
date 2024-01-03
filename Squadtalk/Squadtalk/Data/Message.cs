namespace Squadtalk.Data;

public class Message
{
    public uint Id { get; set; }

    public ApplicationUser Author { get; set; } = default!;
    
    public string ChannelId { get; set; } = default!;
    
    public DateTimeOffset Timestamp { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    public Embed? Embed { get; set; }
}