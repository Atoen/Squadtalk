using Shared.Data.TypedIds;

namespace Shared.Data;

public interface IChatChannel
{
    ChannelId Id { get; }

    string? Name { get; }
    
    IEnumerable<IChatUser> Participants { get; }
    
    IChatMessage? LastMessage { get; }

    int MessagesSince { get; set; }
}