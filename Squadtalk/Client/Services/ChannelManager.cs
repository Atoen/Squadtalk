using Squadtalk.Client.Shared;

namespace Squadtalk.Client.Services;

public class ChannelManager
{
    public static readonly Channel Global = new()
    {
        Name = "Global",
        Id = Guid.Empty
    };
    
    private Channel _current = Global;

    public Channel Current
    {
        get => _current;
        set
        {
            var val = _current;
            _current = value;
            Console.WriteLine(_current);

            if (val != _current)
            {
                ChannelChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? ChannelChanged;

    public Guid CurrentId => Current.Id;
    public string CurrentName => Current.Name;

    public void SelectGlobal() => Current = Global;
    
    public void Select(Channel channel) => Current = channel;
    
    public void Select(string name, Guid id) => Current = new Channel
    {
        Name = name,
        Id = id
    };
}