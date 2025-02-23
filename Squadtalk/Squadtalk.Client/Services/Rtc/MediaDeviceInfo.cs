using JetBrains.Annotations;

namespace Squadtalk.Client.Services.Rtc;

[UsedImplicitly]
internal sealed class MediaDeviceInfo
{
    public string Label { get; set; } = default!;
    public string Id { get; set; } = default!;

    public bool IsUnknown => string.IsNullOrEmpty(Label);

    public string LabelOrDefault(string defaultName) => IsUnknown ? defaultName : Label;
}
