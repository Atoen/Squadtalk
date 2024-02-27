using Shared.Communication;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class MessageModelService<T>(IMessageModelMapper<T> mapper) : IMessageModelService<T>
{
    public TimeSpan MessageSeparationTimespan { get; } = TimeSpan.FromMinutes(5);

    public IList<MessageModel> CreateModelPage(IList<T> inputPage, TextChannelState channelState)
    {
        if (inputPage.Count == 0)
        {
            return ArraySegment<MessageModel>.Empty;
        }

        var page = new MessageModel[inputPage.Count];

        for (var i = 0; i < inputPage.Count; i++)
        {
            var model = CreateModel(inputPage[i], channelState, true);
            page[i] = model;
            channelState.LastPageMessageReceived = model;
        }

        channelState.LastMessageReceived ??= page[^1];

        if (channelState.Messages.Count == 0)
        {
            return page;
        }

        if (page.Length > 0)
        {
            SetMessageSeparateStatus(channelState.Messages[0], page[^1]);
        }
        else
        {
            channelState.Messages[0].IsSeparate = true;
        }

        return page;
    }

    public MessageModel CreateModel(T message, TextChannelState channelState, bool isFromPage)
    {
        var model = mapper.CreateModel(message);
        var previousMessage = isFromPage
            ? channelState.LastPageMessageReceived
            : channelState.LastMessageReceived;
        
        SetMessageSeparateStatus(model, previousMessage);

        return model;
    }

    private void SetMessageSeparateStatus(MessageModel current, MessageModel? other)
    {
        ArgumentNullException.ThrowIfNull(current);
        
        if (other is null)
        {
            current.IsSeparate = true;
            return;
        }
        
        current.IsSeparate = current.Author != other.Author ||
                              current.Timestamp.Subtract(other.Timestamp) > MessageSeparationTimespan;
    }
}