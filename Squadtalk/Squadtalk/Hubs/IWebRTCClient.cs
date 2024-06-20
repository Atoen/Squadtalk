namespace Squadtalk.Hubs;

public interface IWebRTCClient
{
    Task Receive(string message);
}