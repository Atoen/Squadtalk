using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.Entities;

public class Channel
{
    public ChannelId Id { get; set; } = default!;
    
    public List<ApplicationUser> Participants { get; set; } = default!;
    
    public Message? LastMessage { get; set; }
    
    [Owned]
    public class Message
    {
        public required string Content { get; set; }
        public required ChannelId ChannelId { get; set; }
        public required string AuthorName { get; set; }
        public required UserId AuthorId { get; set; }
        public Embed? Embed { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}