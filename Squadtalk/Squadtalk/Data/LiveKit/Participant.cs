namespace Squadtalk.Data.LiveKit;

public class Participant(string sid, string name)
{
    public string Name { get; } = name;
    public string Sid { get; } = sid;
}
