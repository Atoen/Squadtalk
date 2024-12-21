using Shared.Data;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class MessageModelService(IChannelManager channelManager) : IMessageModelService
{
    public TimeSpan MessageSeparationTimespan { get; } = TimeSpan.FromMinutes(5);

    public IList<MessageModel> CreateModelPage(IReadOnlyList<IChatMessage> inputPage, ChannelState channelState)
    {
        if (inputPage.Count == 0)
        {
            return Array.Empty<MessageModel>();
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

    public MessageModel CreateModel(IChatMessage message, ChannelState channelState, bool isFromPage)
    {
        var model = new MessageModel
        {
            Author = channelManager.GetOrCreateUserModel(message.Author),
            Timestamp = message.Timestamp,
            Content = message.Content,
            Embed = message.Embed is { } embed
                ? new EmbedModel
                {
                    Type = embed.Type,
                    Data = embed.Data
                }
                : null
        };

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

        current.IsSeparate = other.IsSystemMessage ||
                             current.Author != other.Author ||
                             current.Timestamp.Subtract(other.Timestamp) > MessageSeparationTimespan;
    }
}
