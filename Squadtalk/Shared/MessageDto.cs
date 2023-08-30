namespace Squadtalk.Shared;

public class MessageDto
{
    public required MessageAuthorDto Author { get; set; }
    public int Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public required string Content { get; set; }
    public EmbedDto? Embed { get; set; }
}