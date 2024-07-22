using Shared.Data;
using Shared.Models;

namespace Shared.Services;

public interface IMessageModelService
{
    TimeSpan MessageSeparationTimespan { get; }
    
    IList<MessageModel> CreateModelPage(IList<IChatMessage> inputPage, ChannelState channelState);

    MessageModel CreateModel(IChatMessage message, ChannelState channelState, bool isFromPage);
}
