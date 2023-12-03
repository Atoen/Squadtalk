namespace Shared.DTOs;

public class MessageDto
{
    public UserDto Author { get; set; } = default!;
    
    public string Content { get; set; } = default!;
    
    public string ChannelId { get; set; } = default!;
    
    public DateTimeOffset Timestamp { get; set; }
}