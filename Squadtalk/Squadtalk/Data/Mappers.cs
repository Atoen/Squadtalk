using Riok.Mapperly.Abstractions;
using Shared.DTOs;

namespace Squadtalk.Data;

[Mapper]
public static partial class Mappers
{
    public static partial MessageDto ToDto(this Message message);
    
    public static partial UserDto ToDto(this ApplicationUser message);
    
    public static partial ChannelDto ToDto(this Channel message);
}