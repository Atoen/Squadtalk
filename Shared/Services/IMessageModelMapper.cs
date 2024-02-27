using Shared.Models;

namespace Shared.Services;

public interface IMessageModelMapper<in TMessage>
{
    MessageModel CreateModel(TMessage message);
}