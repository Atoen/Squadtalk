using Microsoft.EntityFrameworkCore;

namespace Squadtalk.Data;

public class Channel
{
    public required string Id { get; set; }
    
    public required List<ApplicationUser> Participants { get; set; }
    
    public Message? LastMessage { get; set; }
    
    [Owned]
    public class Message
    {
        public required string Content { get; set; }
        public required string ChannelId { get; set; }
        public required string AuthorName { get; set; }
        public required string AuthorId { get; set; }
        public Embed? Embed { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}