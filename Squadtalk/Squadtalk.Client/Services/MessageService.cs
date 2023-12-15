using System.Text;
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
    private readonly ICommunicationManager _communicationManager;
    
    public event Func<string, Task>? MessageReceived;
    
    public MessageService(
        ICommunicationManager communicationManager,
        RestClient restClient,
        ISignalrService signalrService,
        IMessageModelService<MessageDto> modelService,
        ILogger<MessageService> logger)
    {
        _communicationManager = communicationManager;
        _restClient = restClient;
        _signalrService = signalrService;
        _modelService = modelService;
        _logger = logger;

        _signalrService.MessageReceived += HandleIncomingMessage;
    }

    private Task HandleIncomingMessage(MessageDto messageDto)
    {
        _logger.LogInformation("{Author}: {Content}", messageDto.Author.Username, messageDto.Content);
        
        var channel = _communicationManager.GetChannel(messageDto.ChannelId);
        if (channel is null)
        {
            Console.WriteLine("Received message on nonexistent channel");
            return Task.CompletedTask;
        }

        var state = channel.State;
        var message = _modelService.CreateModel(messageDto, state, false);

        state.Messages.Add(message);
        state.LastMessageReceived = message;

        if (state.Cursor == default)
        {
            state.Cursor = DateTimeOffset.UtcNow.UtcTicks;
        }

        return MessageReceived.TryInvoke(messageDto.ChannelId);
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
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(state.Cursor.ToString()));
            restRequest.AddUrlSegment("timestamp", encoded);
        }

        var response = await _restClient.GetAsync<List<MessageDto>>(restRequest, cancellationToken);

        if (response!.Count > 0)
        {
            state.Cursor = response[0].Timestamp.UtcTicks;
        }

        return _modelService.CreateModelPage(response, state);
    }
}