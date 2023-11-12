using Squadtalk.Shared;

namespace Squadtalk.Server.Models;

public class Channel
{
    public Guid Id { get; set; }
    public List<User> Participants { get; set; } = null!;

    public ChannelDto ToDto => new()
    {
        Id = Id,
        Participants = Participants.Select(x => x.ToDto()).ToList()
    };
}
