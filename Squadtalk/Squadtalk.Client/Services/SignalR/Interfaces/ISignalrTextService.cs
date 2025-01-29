using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Squadtalk.Client.Data;

namespace Squadtalk.Client.Services.SignalR.Interfaces;

public interface ISignalrTextService
{
    event Action<MessageDto>? MessageReceived;
    event Action<GroupId, string?>? ChannelNameChanged;

    event Action<GroupDto>? ChannelParticipantsChanged;
    event Func<GroupDto, Task>? AddedToChannel;
    event Func<IEnumerable<GroupDto>, Task>? ChannelsReceived;

    event Action<GroupId, UserId>? UserIsTyping;
    event Action<GroupId, UserId>? UserStoppedTyping;

    Task<SignalrResult> SendMessageAsync(string message, GroupId groupId, CancellationToken cancellationToken = default);

    Task<SignalrResult<List<MessageDto>>> GetMessagePageAsync(GroupId groupId, TextChannelCursor cursor = default, CancellationToken cancellationToken = default);

    Task<SignalrResult<GroupId?>> CreateGroupAsync(IEnumerable<UserId> participants, CancellationToken cancellationToken = default);

    Task<SignalrResult<bool>> AddFriendsToGroupAsync(GroupId groupId, IEnumerable<UserId> friends, CancellationToken cancellationToken = default);

    Task<SignalrResult<bool>> ChangeGroupNameAsync(GroupId groupId, string? newName, CancellationToken cancellationToken = default);

    Task<SignalrResult> UserIsTypingAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<SignalrResult> UserStoppedTypingAsync(GroupId groupId, CancellationToken cancellationToken = default);
}
