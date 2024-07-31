using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public abstract class ChannelModel(ChannelId id) : IStatus
{
    public abstract string Name { get; }

    public abstract UserStatus Status { get; }

    public string? ImageUrl { get; }
    
    public abstract IEnumerable<UserModel> Others { get; }

    public IChatMessage? LastMessage { get; set; }

    public ChannelId Id { get; } = id;

    public ChannelState State { get; } = new();

    public int UnreadMessages => State.UnreadMessages;

    public bool HasUnreadMessages => State.UnreadMessages > 0;
}

public interface IStatus
{
    UserStatus Status { get; }
}
