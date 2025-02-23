namespace Shared.Models;

public abstract record MediaDeviceModel(string Label, string Id)
{
    public bool IsUnknown => string.IsNullOrEmpty(Label);

    public string LabelOrDefault(string defaultName) => IsUnknown ? defaultName : Label;
}

public sealed record MicrophoneModel(string Label, string Id) : MediaDeviceModel(Label, Id);

public sealed record CameraModel(string Label, string Id) : MediaDeviceModel(Label, Id);
