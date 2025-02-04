using Shared.Data.TypedIds;

namespace Shared.Data;

public interface IChatMessage
{
    MessageId Id { get; }

    IChatUser Author { get; }

    GroupId GroupId { get; }

    string Content { get; }

    DateTimeOffset Timestamp { get; }

    IMessageEmbed? Embed { get; }
}
