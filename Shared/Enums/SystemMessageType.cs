namespace Shared.Enums;

public enum SystemMessageType
{
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
        SystemMessageType.ChannelCreated => EmbedData.SystemMessageChannelCreated,
        SystemMessageType.ChannelNameChanged => EmbedData.SystemMessageChannelNameChanged,
        SystemMessageType.CallStarted => EmbedData.SystemMessageCallStarted,
        SystemMessageType.CallEnded => EmbedData.SystemMessageCallEnded,
        SystemMessageType.CallMissed => EmbedData.SystemMessageCallMissed,
        _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
    };

    public static SystemMessageType Parse(string value) => value switch
    {
        EmbedData.SystemMessageChannelCreated => SystemMessageType.ChannelCreated,
        EmbedData.SystemMessageChannelNameChanged => SystemMessageType.ChannelNameChanged,
        EmbedData.SystemMessageCallStarted => SystemMessageType.CallStarted,
        EmbedData.SystemMessageCallEnded => SystemMessageType.CallEnded,
        EmbedData.SystemMessageCallMissed => SystemMessageType.CallMissed,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}
