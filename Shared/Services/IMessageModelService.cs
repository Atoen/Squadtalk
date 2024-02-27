using Shared.Communication;
using Shared.Models;

namespace Shared.Services;

public interface IMessageModelService<TMessage>
{
    TimeSpan MessageSeparationTimespan { get; }
    
    IList<MessageModel> CreateModelPage(IList<TMessage> inputPage, TextChannelState channelState);

    MessageModel CreateModel(TMessage message, TextChannelState channelState, bool isFromPage);
}