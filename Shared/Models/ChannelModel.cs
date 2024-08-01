using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;

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

    public static ChannelModel Create(IChatChannel channel, UserId currentUserId, Func<IChatUser, UserModel> userModelProvider)
    {
        var othersInChannel = channel.Participants.Where(x => x.Id != currentUserId).ToList();

        ChannelModel channelModel = othersInChannel switch
        {
            [var other] => new DirectMessageChannelModel(userModelProvider(other), channel.Id),
            { Count: > 1 } => new GroupChatModel(othersInChannel.Select(userModelProvider), channel.Id, channel.Name),
            _ => throw new InvalidOperationException()
        };

        return channelModel
            .WithLastMessage(channel.LastMessage)
            .WithUnreadMessageCount(channel.MessagesSince);
    }
}

public interface IStatus
{
    UserStatus Status { get; }
}
