using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RestSharp;
using RestSharp.Authenticators;
using Squadtalk.Client.Extensions;
using Squadtalk.Client.Models;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public delegate Task MessageReceived();

public sealed class MessageService
{
    private readonly JwtService _jwtService;
    private readonly RestClient _restClient;
    private readonly JwtAuthenticator _restAuthenticator;
    private readonly TimeSpan _firstMessageTimeSpan = TimeSpan.FromMinutes(5);
    
    private MessageModel? _lastMessageReceived;
    private MessageModel? _lastMessageFormatted;

    private long _pageCursor;

    public List<MessageModel> Messages { get; } = new();

    public event MessageReceived? MessageReceived;

    public MessageService(JwtService jwtService, IWebAssemblyHostEnvironment hostEnvironment)
    {
        _jwtService = jwtService;
        _restAuthenticator = new JwtAuthenticator(_jwtService.Token);
        _restClient = new RestClient(new RestClientOptions
        {
            BaseUrl = new Uri(hostEnvironment.BaseAddress),
            Authenticator = _restAuthenticator
        });
    }

    public async Task HandleIncomingMessage(MessageDto messageDto)
    {
        var message = FormatMessage(messageDto, false);
        _lastMessageReceived = message;
        
        Messages.Add(message);

        if (MessageReceived is not null)
        {
            await MessageReceived();
        }
    }

    public async Task<IList<MessageModel>> GetMessagePageAsync()
    {
        _restAuthenticator.SetBearerToken(_jwtService.Token);
        var restRequest = new RestRequest("api/Message");

        if (_pageCursor != default)
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(_pageCursor.ToString()));
            restRequest.AddQueryParameter("timestamp", encoded);
        }
        
        var response = await _restClient.GetAsync<List<MessageDto>>(restRequest);

        if (response!.Count > 0)
        {
            _pageCursor = response[0].Timestamp.UtcTicks;
        }
        
        var page = FormatMessagePage(response);
        if (Messages.Count > 0)
        {
            if (page.Length > 0)
            {
                CheckIfPreviousMessageWasFirst(Messages[0], page[^1]);
            }
            else
            {
                Messages[0].IsFirst = true;
            }
        }

        return page;
    }

    private void CheckIfPreviousMessageWasFirst(MessageModel previous, MessageModel next)
    {
        previous.IsFirst = previous.Author != next.Author ||
                           previous.Timestamp.Subtract(next.Timestamp) > _firstMessageTimeSpan;
    }

    private MessageModel[] FormatMessagePage(IList<MessageDto> dtoPage)
    {
        var page = new MessageModel[dtoPage.Count];

        for (var i = 0; i < dtoPage.Count; i++)
        {
            var model = FormatMessage(dtoPage[i], true);
            page[i] = model;
            _lastMessageFormatted = model;
        }

        _lastMessageReceived ??= page[^1];

        return page;
    }

    private MessageModel FormatMessage(MessageDto messageDto, bool isFromPage)
    {
        var model = messageDto.ToModel();
        var toCompare = isFromPage ? _lastMessageFormatted : _lastMessageReceived;

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