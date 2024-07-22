namespace Shared.Enums;

public enum SystemMessageType
{
    None,
    ChannelCreated,
    ChannelNameChanged,
    CallStarted,
    CallEnded,
    CallMissed
}

public static class SystemMessageTypeHelper
{
    public static string Format(SystemMessageType messageType) => messageType switch
    {
        SystemMessageType.ChannelCreated => EmbedData.SystemMessageTypeChannelCreated,
        SystemMessageType.ChannelNameChanged => EmbedData.SystemMessageTypeChannelNameChanged,
        SystemMessageType.CallStarted => EmbedData.SystemMessageTypeCallStarted,
        SystemMessageType.CallEnded => EmbedData.SystemMessageTypeCallEnded,
        SystemMessageType.CallMissed => EmbedData.SystemMessageTypeCallMissed,
        _ => string.Empty,
    };

    public static SystemMessageType Parse(string value) => value switch
    {
        EmbedData.SystemMessageTypeChannelCreated => SystemMessageType.ChannelCreated,
        EmbedData.SystemMessageTypeChannelNameChanged => SystemMessageType.ChannelNameChanged,
        EmbedData.SystemMessageTypeCallStarted => SystemMessageType.CallStarted,
        EmbedData.SystemMessageTypeCallEnded => SystemMessageType.CallEnded,
        EmbedData.SystemMessageTypeCallMissed => SystemMessageType.CallMissed,
        _ => SystemMessageType.None
    };
}
