namespace Shared.Data;

public interface IChatChannel
{
    ChannelId Id { get; }
    
    IEnumerable<IChatUser> Participants { get; }
    
    IChatMessage? LastMessage { get; }
}