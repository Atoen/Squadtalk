namespace Shared.Models;

public class MessageModel
{
    public string Author { get; set; } = default!;
    
    public string Content { get; set; } = default!;
    
    public DateTimeOffset Timestamp { get; set; }
    
    public bool IsSeparate { get; set; }
}