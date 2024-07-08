using Shared.Data.TypedIds;

namespace Squadtalk.Data.LiveKit;

public class Participant(string sid, string name, string idString)
{
    public string Name { get; } = name;
    public string Sid { get; } = sid;
    public UserId Id { get; } = UserId.Parse(idString);
}
