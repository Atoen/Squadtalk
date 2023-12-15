using Shared.DTOs;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class DtoMessageModelMapper : IMessageModelMapper<MessageDto>
{
    public MessageModel CreateModel(MessageDto dto)
    {
        return new MessageModel
        {
            Author = dto.Author.Username,
            Timestamp = dto.Timestamp,
            Content = dto.Content
        };
    }
}