using RestSharp;
using RestSharp.Authenticators;
using Squadtalk.Client.Extensions;
using Squadtalk.Client.Models;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public sealed class MessageService
{
    private readonly JWTService _jwtService;
    private readonly RestClient _restClient;
    private readonly JwtAuthenticator _restAuthenticator;
    private readonly TimeSpan _firstMessageTimeSpan = TimeSpan.FromMinutes(5);

    private int _offset;
    private MessageModel? _lastMessage;

    public Action<MessageModel>? MessageReceived { get; set; }

    public MessageService(JWTService jwtService)
    {
        _jwtService = jwtService;
        _restAuthenticator = new JwtAuthenticator(_jwtService.Token);
        _restClient = new RestClient(new RestClientOptions
        {
            BaseUrl = new Uri("http://squadtalk.ddns.net"),
            Authenticator = _restAuthenticator
        });
    }

    public void HandleIncomingMessage(MessageDto messageDto)
    {
        var formatted = FormatMessage(messageDto);
        _lastMessage = formatted;

        MessageReceived?.Invoke(formatted);
    }

    public async Task<IList<MessageModel>> GetMessagePageAsync(int requestOffset)
    {
        var pageOffset = _offset + requestOffset;

        _restAuthenticator.SetBearerToken(_jwtService.Token);
        var restRequest = new RestRequest($"api/Message?offset={pageOffset}");
        var response = await _restClient.GetAsync<List<MessageDto>>(restRequest);
        var responseCount = response!.Count;
        
        var page = new MessageModel[responseCount];

        for (var i = responseCount - 1; i >= 0; i--)
        {
            var model = FormatMessage(response[i]);
            page[i] = model;
            _lastMessage = model;
        }

        return page;
    }

    public void CheckIfIsFirst(MessageModel previous, MessageModel next)
    {
        previous.IsFirst = previous.Author != next.Author ||
                           previous.Timestamp.Subtract(next.Timestamp) > _firstMessageTimeSpan;
    }

    private MessageModel FormatMessage(MessageDto messageDto)
    {
        var model = messageDto.ToModel();

        if (_lastMessage is null)
        {
            model.IsFirst = true;
            return model;
        }

        if (model.Author != _lastMessage.Author ||
            model.Timestamp.Subtract(_lastMessage.Timestamp) > _firstMessageTimeSpan)
        {
            model.IsFirst = true;
        }

        return model;
    }
}