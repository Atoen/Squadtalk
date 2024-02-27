namespace Squadtalk.Shared;

public class MessageDto
{
    public required UserDto Author { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public required string Content { get; set; }
    public Guid ChannelId { get; set; }
    public EmbedDto? Embed { get; set; }
}