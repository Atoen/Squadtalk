namespace Shared.Models;

public class MediaDeviceModel
{
    public string Label { get; set; } = default!;
    public string Id { get; set; } = default!;

    public bool IsUnknown => string.IsNullOrEmpty(Label);

    public string LabelOrDefault => IsUnknown ? "Unknown device" : Label;
}
