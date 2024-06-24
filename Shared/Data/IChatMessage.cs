using Shared.Data.TypedIds;

namespace Shared.Data;

public interface IChatMessage
{
    IChatUser Author { get; }
    
    ChannelId ChannelId { get; }
    
    string Content { get; }
    
    DateTimeOffset Timestamp { get; }
    
    IMessageEmbed? Embed { get; }
}