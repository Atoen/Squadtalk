using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;
using Shared.Reactive;

namespace Shared.Models;

public abstract class ChatModel : Observable<ChatModel>, IKeyId<GroupId>
{
    public const string GlobalChanelIdValue = "global";
    public static readonly GroupId GlobalChatId = new(GlobalChanelIdValue);

    private string? _imageUrl;
    private bool _hasActiveCall;

    public abstract string Name { get; }

    public abstract UserStatus Status { get; }

    public string? ImageUrl
    {
        get => _imageUrl;
        set => SetField(ref _imageUrl, value);
    }

    public bool HasActiveCall
    {
        get => _hasActiveCall;
        set => SetField(ref _hasActiveCall, value);
    }

    public abstract IEnumerable<GroupParticipantModel> Participants { get; }

    public abstract IEnumerable<GroupParticipantModel> Others { get; }

    public abstract GroupParticipantModel LocalUser { get; }

    public IChatMessage? LastMessage { get; set; }

    public GroupId Id { get; }

    public ChannelState State { get; }

    public GroupId Key => Id;

    protected ChatModel(GroupId id)
    {
        Id = id;
        State = new ChannelState(this);
    }

    public virtual void UpdateParticipants(IEnumerable<GroupParticipantModel> updatedParticipants) { }

    public virtual void UpdateParticipantRole(UserId userId, GroupRole groupRole) { }

    public static ChatModel CreateGlobalChat(UserModel localUser) => new GlobalChatModel(localUser);

    public static ChatModel Create(IChatGroup group, Func<IGroupParticipant, GroupParticipantModel> participantModelProvider)
    {
        var participantModels = group.Participants.Select(participantModelProvider);

        ChatModel chatModel = group.Type switch
        {
            ChatType.DirectMessage => new DirectMessageModel(participantModels, group.Id),
            ChatType.GroupChat => new GroupChatModel(participantModels, group.Id, group.CustomName),
            _ => throw new ArgumentOutOfRangeException(nameof(group), nameof(group.Type))
        };

        return chatModel
            .WithLastMessage(group.LastMessage)
            .WithUnreadMessageCount(group.MessagesSince);
    }
}
