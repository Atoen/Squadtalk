using Microsoft.JSInterop;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public delegate void FileSelectedHandler(string filename, string filesize);

public delegate void UploadStartedHandler(string filename, string filesize);

public delegate void UploadSpeedUpdateHandler(string uploadSpeed);

public delegate void UploadEndedHandler(string? error);

public delegate void RemovedSelectedFileHandler();

public class FileTransferService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly JwtService _jwtService;

    private bool _initialized;

    private IJSObjectReference? _module;
    private DotNetObjectReference<FileTransferService>? _objectReference;
    private bool _uploadInProgress;

    public FileTransferService(JwtService jwtService, IJSRuntime jsRuntime)
    {
        _jwtService = jwtService;
        _jsRuntime = jsRuntime;
    }

    public bool CanStartUpload => Selected && !_uploadInProgress;
    public bool Selected { get; private set; }
    public string? SelectedFileSize { get; private set; }
    public string? SelectedFilename { get; private set; }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null) await _module.DisposeAsync();

        _objectReference?.Dispose();
    }

    public event FileSelectedHandler? FileSelected;
    public event UploadStartedHandler? UploadStarted;
    public event UploadEndedHandler? UploadEnded;
    public event UploadSpeedUpdateHandler? UploadSpeedUpdated;
    public event RemovedSelectedFileHandler? RemovedSelectedFile;

    public async Task InitializeModuleAsync()
    {
        if (_initialized) return;
        _initialized = true;

        _objectReference = DotNetObjectReference.Create(this);

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/FileTransfer.js");
        await _module.InvokeVoidAsync("initialize", _objectReference);

        _jwtService.TokenUpdatedAsync += async token =>
            await _module.InvokeVoidAsync("updateJwt", token);
    }

    public async Task<bool> UploadFileAsync(Guid channelId)
    {
        if (_uploadInProgress) return false;

        await _module!.InvokeVoidAsync("uploadSelectedFile", channelId);

        return true;
    }

    public async Task CancelUpload()
    {
        if (!_uploadInProgress) return;

        await _module!.InvokeVoidAsync("CancelUpload");
    }

    public async Task RemoveSelectedFile()
    {
        if (!Selected) return;

        Selected = false;

        SelectedFilename = null;
        SelectedFileSize = null;

        await _module!.InvokeVoidAsync("removeSelectedFile");
        RemovedSelectedFile?.Invoke();
    }

    [JSInvokable]
    public string GetJwt()
    {
        return _jwtService.Token;
    }

    [JSInvokable]
    public void FileSelectedCallback(string filename, string size)
    {
        Selected = true;

        SelectedFilename = filename;
        SelectedFileSize = MessageExtensions.ConvertToHumanReadableSize(size);

        FileSelected?.Invoke(SelectedFilename, SelectedFileSize);
    }

    [JSInvokable]
    public void UploadStartedCallback(string filename, string size)
    {
        Selected = false;
        _uploadInProgress = true;

        var niceSize = MessageExtensions.ConvertToHumanReadableSize(size);
        UploadStarted?.Invoke(filename, niceSize);
    }

    [JSInvokable]
    public void UploadEndedCallback(string? error)
    {
        _uploadInProgress = false;

        UploadEnded?.Invoke(error);
    }

    [JSInvokable]
    public void UpdateUploadSpeedCallback(string bytesPerSecond)
    {
        var speed = MessageExtensions.ConvertToHumanReadableSize(bytesPerSecond);
        UploadSpeedUpdated?.Invoke(speed);
    }
}