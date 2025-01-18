using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Squadtalk.Client.Data;

namespace Squadtalk.Client.Services.SignalR.Interfaces;

public interface ISignalrTextService
{
    event Action<MessageDto>? MessageReceived;
    event Action<ChannelId, string?>? ChannelNameChanged;

    event Func<ChannelDto, Task>? AddedToChannel;
    event Func<IEnumerable<ChannelDto>, Task>? ChannelsReceived;

    event Action<ChannelId, UserId>? UserIsTyping;
    event Action<ChannelId, UserId>? UserStoppedTyping;

    Task<SignalrResult> SendMessageAsync(string message, ChannelId channelId, CancellationToken cancellationToken = default);

    Task<SignalrResult<List<MessageDto>>> GetMessagePageAsync(ChannelId channelId, TextChannelCursor cursor = default, CancellationToken cancellationToken = default);

    Task<SignalrResult<ChannelId?>> CreateChannelAsync(IEnumerable<UserId> participants, CancellationToken cancellationToken = default);

    Task<SignalrResult<bool>> ChangeChannelNameAsync(ChannelId channelId, string? newName, CancellationToken cancellationToken);

    Task<SignalrResult> UserIsTypingAsync(ChannelId channelId);

    Task<SignalrResult> UserStoppedTypingAsync(ChannelId channelId);
}
