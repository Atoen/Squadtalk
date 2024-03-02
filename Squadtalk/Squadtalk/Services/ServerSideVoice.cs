using Shared.Services;

namespace Squadtalk.Services;

public class ServerSideVoice : IVoiceChatService
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public bool Connected { get; }
    public Task ConnectAsync()
    {
        return Task.CompletedTask;
    }

    public Task StartStreamAsync()
    {
        throw new InvalidOperationException();
    }

    public Task StopStreamAsync()
    {
        throw new InvalidOperationException();
    }

    public Task<double> MeasurePingAsync()
    {
        throw new InvalidOperationException();
    }

    public event Action<byte[]>? PacketReceived;
    public Task StartVoiceCallAsync(List<string> invitedIds)
    {
        throw new InvalidOperationException();
    }
}