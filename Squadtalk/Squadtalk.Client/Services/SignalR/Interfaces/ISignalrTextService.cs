using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Squadtalk.Client.Data;

namespace Squadtalk.Client.Services.SignalR.Interfaces;

public interface ISignalrTextService
{
    event Action<MessageDto>? MessageReceived;
    event Action<GroupId, string?>? ChannelNameChanged;

    event Action<GroupDto>? GroupParticipantsChanged;
    event Action<GroupDto>? AddedToGroup;
    event Action<IEnumerable<GroupDto>>? ChannelsReceived;

    event Action<GroupId, UserId>? UserIsTyping;
    event Action<GroupId, UserId>? UserStoppedTyping;

    event Action<GroupId, UserId, GroupRole>? ParticipantRoleChanged;
    event Action<GroupId>? GroupDeleted;

    Task<NetworkResult> SendMessageAsync(string message, GroupId groupId, CancellationToken cancellationToken = default);

    Task<NetworkResult<List<MessageDto>>> GetMessagePageAsync(GroupId groupId, TextChannelCursor cursor = default, CancellationToken cancellationToken = default);

    Task<NetworkResult<GroupDto?>> CreateGroupAsync(IEnumerable<UserId> participants, CancellationToken cancellationToken = default);

    Task<NetworkResult<HubResult>> AddFriendsToGroupAsync(GroupId groupId, IEnumerable<UserId> friends, CancellationToken cancellationToken = default);

    Task<NetworkResult<HubResult>> ChangeGroupNameAsync(GroupId groupId, string? newName, CancellationToken cancellationToken = default);

    Task<NetworkResult<HubResult>> KickUserAsync(GroupId groupId, UserId userId, CancellationToken cancellationToken = default);

    Task<NetworkResult> UserIsTypingAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<NetworkResult> UserStoppedTypingAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<NetworkResult> MarkMessageSeenAsync(GroupId groupId, MessageId messageId, CancellationToken cancellationToken = default);
}
