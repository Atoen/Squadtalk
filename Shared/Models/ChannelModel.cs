using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;
using Shared.Reactive;

namespace Shared.Models;

public abstract class ChannelModel : Observable<ChannelModel>
{
    private string? _imageUrl;

    public abstract string Name { get; }

    public abstract UserStatus Status { get; }

    public string? ImageUrl
    {
        get => _imageUrl;
        set => SetField(ref _imageUrl, value);
    }

    public abstract IEnumerable<UserModel> Others { get; }

    public IChatMessage? LastMessage { get; set; }

    public ChannelId Id { get; }

    public ChannelState State { get; }

    public int UnreadMessages => State.UnreadMessages;

    public bool HasUnreadMessages => State.UnreadMessages > 0;

    public bool IsSomeoneTyping => State.TypingUsers.Count != 0;

    public IReadOnlyCollection<TypingUser> TypingUsers => State.TypingUsers.Typing;

    protected ChannelModel(ChannelId id)
    {
        Id = id;
        State = new ChannelState(this);
    }

    public virtual void UpdateParticipants(IEnumerable<UserModel> updatedParticipants) { }

    public static ChannelModel Create(IChatChannel channel, UserId currentUserId, Func<IChatUser, UserModel> userModelProvider)
    {
        var othersInChannel = channel.Participants.Where(x => x.Id != currentUserId).ToList();

        ChannelModel channelModel = othersInChannel switch
        {
            [var other] => new DirectMessageChannelModel(userModelProvider(other), channel.Id),
            _ => new GroupChatModel(othersInChannel.Select(userModelProvider), channel.Id, channel.Name)
        };

        return channelModel
            .WithLastMessage(channel.LastMessage)
            .WithUnreadMessageCount(channel.MessagesSince);
    }
}
