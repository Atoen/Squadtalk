namespace Shared.Communication;

public abstract class TextChannel(string id) : ICommunicationTab
{
    public abstract string Name { get; }
    public virtual bool Selected { get; set; }
    
    public string Id { get; } = id;

    public TextChannelState State { get; } = new();
}