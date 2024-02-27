using Riok.Mapperly.Abstractions;
using Shared.DTOs;

namespace Squadtalk.Data;

[Mapper]
public static partial class Mappers
{
    public static partial MessageDto ToDto(this Message message);
    
    [MapProperty(nameof(ApplicationUser.UserName), nameof(UserDto.Username))]
    public static partial UserDto ToDto(this ApplicationUser user);
    
    public static partial ChannelDto ToDto(this Channel user);

    public static partial EmbedDto ToDto(this Embed embed);
}