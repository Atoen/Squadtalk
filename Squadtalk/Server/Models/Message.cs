using Squadtalk.Shared;

namespace Squadtalk.Server.Models;

public class Message
{
    public int Id { get; set; }
    public User Author { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public Guid ChannelId { get; set; } 
    public DateTimeOffset Timestamp { get; set; }
    public string Content { get; set; } = string.Empty;
    public Embed? Embed { get; set; }

    public MessageDto ToDto()
    {
        return new MessageDto
        {
            Author = Author.ToDto(),
            Id = Id,
            Timestamp = Timestamp,
            Content = Content,
            Embed = Embed?.ToDto()
        };
    }
}
