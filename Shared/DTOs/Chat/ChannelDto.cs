using MessagePack;
using Shared.Data;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class ChannelDto : IChatChannel
{
    [Key(0)]
    public ChannelId Id { get; set; } = default!;

    [Key(1)]
    public string? Name { get; set; }

    [Key(2)]
    public List<UserDto> Participants { get; set; } = default!;

    [Key(3)]
    public MessageDto? LastMessage { get; set; }

    [Key(4)]
    public int MessagesSince { get; set; }

    [IgnoreMember]
    IEnumerable<IChatUser> IChatChannel.Participants => Participants;
    
    [IgnoreMember]
    IChatMessage? IChatChannel.LastMessage => LastMessage;
}
