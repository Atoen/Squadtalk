using Shared.Models;
using Shared.Services;
using Squadtalk.Data;

namespace Squadtalk.Services;

public class MessageModelMapper : IMessageModelMapper<Message>
{
    public MessageModel CreateModel(Message message)
    {
        return new MessageModel
        {
            Author = message.Author.UserName!,
            Timestamp = message.Timestamp,
            Content = message.Content
        };
    }
}