namespace Shared.Services;

public interface IVoiceChatService : IAsyncDisposable
{
    bool Connected { get; }
    
    Task ConnectAsync();

    Task StartStreamAsync();

    Task StopStreamAsync();

    Task<double> MeasurePingAsync();
    
    event Action<byte[]>? PacketReceived;

    Task StartVoiceCallAsync(List<string> invitedIds);
}