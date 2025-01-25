using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Extensions;
using Shared.Signalr;
using Shared.Signalr.Clients;
using Squadtalk.Client.Data;
using Squadtalk.Client.Services.SignalR.Interfaces;

namespace Squadtalk.Client.Services.SignalR;

internal sealed partial class SignalrService : ISignalrTextService
{
    public event Action<MessageDto>? MessageReceived;
    public event Action<ChannelId, string?>? ChannelNameChanged;

    public event Func<ChannelDto, Task>? AddedToChannel;
    public event Func<IEnumerable<ChannelDto>, Task>? ChannelsReceived;

    public event Action<ChannelId, UserId>? UserIsTyping;
    public event Action<ChannelId, UserId>? UserStoppedTyping;

    public Task<SignalrResult> SendMessageAsync(string message, ChannelId channelId, CancellationToken cancellationToken)
    {
        return SendAsync(HubMethods.SendMessage, message, channelId, cancellationToken);
    }

    public Task<SignalrResult<List<MessageDto>>> GetMessagePageAsync(ChannelId channelId, TextChannelCursor cursor = default, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<List<MessageDto>>(HubMethods.GetMessagePage, channelId, cursor, cancellationToken);
    }

    public Task<SignalrResult<ChannelId?>> CreateChannelAsync(IEnumerable<UserId> participants, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<ChannelId?>(HubMethods.CreateChannel, participants.ToList(), cancellationToken);
    }

    public Task<SignalrResult<bool>> ChangeChannelNameAsync(ChannelId channelId, string? newName, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<bool>(HubMethods.ChangeChannelName, newName, cancellationToken);
    }

    public Task<SignalrResult> UserIsTypingAsync(ChannelId channelId, CancellationToken cancellationToken = default)
    {
        return SendAsync(HubMethods.IsTyping, channelId, cancellationToken);
    }

    public Task<SignalrResult> UserStoppedTypingAsync(ChannelId channelId, CancellationToken cancellationToken = default)
    {
        return SendAsync(HubMethods.StoppedTyping, channelId, cancellationToken);
    }

    private void RegisterTextHandlers()
    {
        _connection.On<MessageDto>(nameof(ITextChatClient.ReceivedMessage), message =>
            MessageReceived?.Invoke(message));

        _connection.On<IList<ChannelDto>>(nameof(ITextChatClient.ReceivedChannels), channels =>
            ChannelsReceived.TryInvoke(channels));

        _connection.On<ChannelDto>(nameof(ITextChatClient.AddedToChannel), channel =>
            AddedToChannel.TryInvoke(channel));

        _connection.On<ChannelId, string?>(nameof(ITextChatClient.ChannelNameChanged), (channelId, newName) =>
            ChannelNameChanged?.Invoke(channelId, newName));

        _connection.On<ChannelId, UserId>(nameof(ITextChatClient.UserIsTyping), (channelId, userId) =>
            UserIsTyping?.Invoke(channelId, userId));

        _connection.On<ChannelId, UserId>(nameof(ITextChatClient.UserStoppedTyping), (channelId, userId) =>
            UserStoppedTyping?.Invoke(channelId, userId));
    }
}
