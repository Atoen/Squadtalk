namespace Squadtalk.Server.Models;

public class Channel
{
    public Guid Id { get; set; }
    public List<User> Participants { get; set; } = null!;
}