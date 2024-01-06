using Shared.DTOs;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class DtoMessageModelMapper : IMessageModelMapper<MessageDto>
{
    public MessageModel CreateModel(MessageDto dto)
    {
        var model = new MessageModel
        {
            Author = dto.Author.Username,
            Timestamp = dto.Timestamp,
            Content = dto.Content,
        };

        if (dto.Embed is { } embed)
        {
            model.Embed = new EmbedModel
            {
                Type = embed.Type,
                Data = embed.Data
            };
        }

        return model;
    }
}