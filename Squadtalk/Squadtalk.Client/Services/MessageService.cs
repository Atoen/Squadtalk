using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RestSharp;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class MessageService : IMessageService
{
    private readonly RestClient _restClient;
    private readonly ILogger<MessageService> _logger;
    private readonly ISignalrService _signalrService;
    private readonly IMessageModelService<MessageDto> _modelService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ICommunicationManager _communicationManager;
    
    public event Func<string, Task>? MessageReceived;
    
    public MessageService(
        ICommunicationManager communicationManager,
        RestClient restClient,
        ISignalrService signalrService,
        IMessageModelService<MessageDto> modelService,
        AuthenticationStateProvider authenticationStateProvider,
        ILogger<MessageService> logger)
    {
        _communicationManager = communicationManager;
        _restClient = restClient;
        _signalrService = signalrService;
        _modelService = modelService;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;

        _signalrService.MessageReceived += HandleIncomingMessage;
    }

    private async Task HandleIncomingMessage(MessageDto messageDto)
    {
        _logger.LogInformation("{Author}: {Content}", messageDto.Author.Username, messageDto.Content);
        
        var channel = _communicationManager.GetChannel(messageDto.ChannelId);
        if (channel is null)
        {
            _logger.LogWarning("Received message on nonexistent channel id: {Id}", messageDto.ChannelId);
            return;
        }

        var state = channel.State;

        if (_communicationManager.CurrentChannel != channel)
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var id = authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier);

            if (messageDto.Author.Id != id)
            {
                state.UnreadMessages++;
            }
        }
        
        var message = _modelService.CreateModel(messageDto, state, false);

        state.Messages.Add(message);
        state.LastMessageReceived = message;

        if (state.Cursor == default)
        {
            state.Cursor = DateTimeOffset.UtcNow.UtcTicks;
        }

        await MessageReceived.TryInvoke(messageDto.ChannelId);
    }
    
    public async Task<IList<MessageModel>> GetMessagePageAsync(string channelId, CancellationToken cancellationToken)
    {
        var channel = _communicationManager.GetChannel(channelId);

        if (channel is null or { State.ReachedEnd: true })
        {
            return ArraySegment<MessageModel>.Empty;
        }
        
        var restRequest = new RestRequest("api/message/{channel}/{timestamp}")
            .AddUrlSegment("channel", channelId);

        var state = channel.State;

        if (state.Cursor != default)
        {
            restRequest.AddUrlSegment("timestamp", state.Cursor.ToString().ToBase64(true));
        }

        var response = await _restClient.GetAsync<List<MessageDto>>(restRequest, cancellationToken);

        if (response!.Count > 0)
        {
            state.Cursor = response[0].Timestamp.UtcTicks;
        }

        return _modelService.CreateModelPage(response, state);
    }
}