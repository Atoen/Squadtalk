using Squadtalk.Client.Shared;

namespace Squadtalk.Client.Models.Communication;

public abstract class Channel : ICommunicationTab
{
    public Guid Id { get; }

    public abstract string Name { get; }
    public virtual bool IsSelected { get; set; }
    
    public ChannelState State { get; }

    protected Channel(Guid id)
    {
        Id = id;
        State = new ChannelState();
    }
}