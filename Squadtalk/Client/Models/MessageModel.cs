namespace Squadtalk.Client.Models;

public class MessageModel
{
    public required string Author { get; set; }
    public required string Content { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    public bool IsFirst { get; set; }
    public EmbedModel? Embed { get; set; }
}