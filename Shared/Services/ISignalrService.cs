namespace Shared.Services;

public interface ISignalrService : ISignalrTextService, ISignalrVoiceService, IAsyncDisposable
{
    const string Online = "Online";
    const string Connecting = "Connecting";
    const string Reconnecting = "Reconnecting";
    const string Disconnected = "Disconnected";
    const string Offline = "Offline";
    
    string ConnectionStatus { get; }
    
    bool Connected { get; }
    
    Task ConnectAsync();
}