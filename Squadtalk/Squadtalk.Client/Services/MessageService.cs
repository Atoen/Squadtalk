using System.Text;
using Microsoft.AspNetCore.Components;
using RestSharp;
using Shared.Communication;
using Shared.DTOs;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public class MessageService : IMessageService
{
    private readonly TimeSpan _separateMessageTimeSpan = TimeSpan.FromMinutes(1);
    
    private readonly NavigationManager _navigationManager;
    private readonly RestClient _restClient;
    private readonly ILogger<MessageService> _logger;
    
    private readonly ISignalrService _signalrService;
    private readonly ICommunicationManager _communicationManager;

    private readonly List<MessageModel> _empty = [];
    
    public event Func<string, Task>? MessageReceived;
    
    public MessageService(
        ICommunicationManager communicationManager,
        NavigationManager navigationManager,
        RestClient restClient,
        ISignalrService signalrService,
        ILogger<MessageService> logger)
    {
        _communicationManager = communicationManager;
        _navigationManager = navigationManager;
        _restClient = restClient;
        _signalrService = signalrService;
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
        var message = FormatMessage(state, messageDto, false);

        state.Messages.Add(message);
        state.LastMessageReceived = message;

        if (state.Cursor == default)
        {
            state.Cursor = DateTimeOffset.UtcNow.UtcTicks;
        }

        return MessageReceived is not null
            ? MessageReceived(messageDto.ChannelId)
            : Task.CompletedTask;
    }
    
    public async Task<IList<MessageModel>> GetMessagePageAsync(string channelId)
    {
        var channel = _communicationManager.GetChannel(channelId);

        if (channel is null or { State.ReachedEnd: true })
        {
            return _empty;
        }
        
        var restRequest = new RestRequest("api/message/{channel}/{timestamp}")
            .AddUrlSegment("channel", channelId);

        var state = channel.State;

        if (state.Cursor != default)
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(state.Cursor.ToString()));
            restRequest.AddUrlSegment("timestamp", encoded);
        }

        var response = await _restClient.GetAsync<List<MessageDto>>(restRequest);

        if (response!.Count > 0)
        {
            state.Cursor = response[0].Timestamp.UtcTicks;
        }

        var page = FormatMessagePage(state, response);
        if (state.Messages.Count == 0)
        {
            return page;
        }

        if (page.Length > 0)
        {
            CheckIfPreviousMessageWasFirst(state.Messages[0], page[^1]);
        }
        else
        {
            state.Messages[0].IsSeparate = true;
        }

        return page;
    }
    
    private void CheckIfPreviousMessageWasFirst(MessageModel previous, MessageModel next)
    {
        previous.IsSeparate = previous.Author != next.Author ||
                           previous.Timestamp.Subtract(next.Timestamp) > _separateMessageTimeSpan;
    }

    private MessageModel[] FormatMessagePage(TextChannelState channelState, IList<MessageDto> dtoPage)
    {
        if (dtoPage.Count == 0)
        {
            return Array.Empty<MessageModel>();
        }

        var page = new MessageModel[dtoPage.Count];

        for (var i = 0; i < dtoPage.Count; i++)
        {
            var model = FormatMessage(channelState, dtoPage[i], true);
            page[i] = model;
            channelState.LastMessageFormatted = model;
        }

        channelState.LastMessageReceived ??= page[^1];

        return page;
    }

    private MessageModel FormatMessage(TextChannelState channelState, MessageDto messageDto, bool isFromPage)
    {
        var model = messageDto.ToModel();
        var toCompare = isFromPage
            ? channelState.LastMessageFormatted
            : channelState.LastMessageReceived;

        if (toCompare is null)
        {
            model.IsSeparate = true;
            return model;
        }

        if (model.Author != toCompare.Author ||
            model.Timestamp.Subtract(toCompare.Timestamp) > _separateMessageTimeSpan)
        {
            model.IsSeparate = true;
        }

        return model;
    }
}