using Shared.Communication;
using Shared.Data;
using Shared.Models;

namespace Shared.Services;

public interface IMessageModelService
{
    TimeSpan MessageSeparationTimespan { get; }
    
    IList<MessageModel> CreateModelPage(IList<IChatMessage> inputPage, TextChannelState channelState);

    MessageModel CreateModel(IChatMessage message, TextChannelState channelState, bool isFromPage);
}