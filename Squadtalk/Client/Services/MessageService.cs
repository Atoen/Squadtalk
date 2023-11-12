using System.Collections.Concurrent;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RestSharp;
using RestSharp.Authenticators;
using Squadtalk.Client.Extensions;
using Squadtalk.Client.Models;
using Squadtalk.Client.Shared;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public delegate Task MessageReceived(Guid channelId);

public sealed class MessageService
{
    // private readonly ConcurrentDictionary<Guid, ChannelState> _channels = new();
    private readonly CommunicationManager _communicationManager;
    
    private readonly TimeSpan _firstMessageTimeSpan = TimeSpan.FromMinutes(5);
    
    private readonly JwtService _jwtService;
    private readonly JwtAuthenticator _restAuthenticator;
    private readonly RestClient _restClient;

    public MessageService(JwtService jwtService, IWebAssemblyHostEnvironment hostEnvironment,
        CommunicationManager communicationManager)
    {
        _jwtService = jwtService;
        _communicationManager = communicationManager;
        _restAuthenticator = new JwtAuthenticator(_jwtService.Token);
        _restClient = new RestClient(new RestClientOptions
        {
            BaseUrl = new Uri(hostEnvironment.BaseAddress),
            Authenticator = _restAuthenticator
        });
    }

    // public List<MessageModel> Messages => ChannelState.Messages;
    //
    // public ChannelState ChannelState => GetChannelById(_communicationManager.CurrentChannel.Id);
    //
    // private ChannelState GetChannelById(Guid id)
    // {
    //     if (!_channels.ContainsKey(id))
    //     {
    //         _channels[id] = new ChannelState(id);
    //     }
    //
    //     return _channels[id];
    // }

    public event MessageReceived? MessageReceived;

    public async Task HandleIncomingMessage(MessageDto messageDto)
    {
        var channel = _communicationManager.GetChannel(messageDto.ChannelId);
        if (channel is null)
        {
            Console.WriteLine("Received message on nonexistent channel");
            return;
        }

        var state = channel.State;
        var message = FormatMessage(state, messageDto, false);

        state.Messages.Add(message);
        state.LastMessageReceived = message;

        if (state.Cursor == default)
        {
            state.Cursor = DateTimeOffset.UtcNow.UtcTicks;
        }

        if (MessageReceived is not null)
        {
            await MessageReceived(messageDto.ChannelId);
        }
    }

    private readonly List<MessageModel> _empty = new();

    public async Task<IList<MessageModel>> GetMessagePageAsync(Guid channelId)
    {
        var channel = _communicationManager.GetChannel(channelId);

        if (channel is null or { State.ReachedEnd: true })
        {
            return _empty;
        }
        
        _restAuthenticator.SetBearerToken(_jwtService.Token);
        
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
            state.Messages[0].IsFirst = true;
        }

        return page;
    }

    private void CheckIfPreviousMessageWasFirst(MessageModel previous, MessageModel next)
    {
        previous.IsFirst = previous.Author != next.Author ||
                           previous.Timestamp.Subtract(next.Timestamp) > _firstMessageTimeSpan;
    }

    private MessageModel[] FormatMessagePage(ChannelState channelState, IList<MessageDto> dtoPage)
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

    private MessageModel FormatMessage(ChannelState channelState, MessageDto messageDto, bool isFromPage)
    {
        var model = messageDto.ToModel();
        var toCompare = isFromPage
            ? channelState.LastMessageFormatted
            : channelState.LastMessageReceived;

        if (toCompare is null)
        {
            model.IsFirst = true;
            return model;
        }

        if (model.Author != toCompare.Author ||
            model.Timestamp.Subtract(toCompare.Timestamp) > _firstMessageTimeSpan)
        {
            model.IsFirst = true;
        }

        return model;
    }
}