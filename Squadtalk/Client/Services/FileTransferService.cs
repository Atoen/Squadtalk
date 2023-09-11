using Microsoft.JSInterop;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public delegate void FileSelectedHandler(string filename, string filesize);

public delegate void UploadStartedHandler(string filename, string filesize);

public delegate void UploadSpeedUpdateHandler(string uploadSpeed);

public delegate void UploadEndedHandler(string? error);

public class FileTransferService : IAsyncDisposable
{
    private readonly JwtService _jwtService;
    private readonly IJSRuntime _jsRuntime;
    
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<FileTransferService>? _objectReference;
    
    private bool _uploadInProgress;
    public bool CanStartUpload => Selected && !_uploadInProgress;
    public bool Selected { get; private set; }
    public string? SelectedFileSize { get; private set; }
    public string? SelectedFilename { get; private set; }

    public event FileSelectedHandler? FileSelected; 
    public event UploadStartedHandler? UploadStarted;
    public event UploadEndedHandler? UploadEnded;
    public event UploadSpeedUpdateHandler? UploadSpeedUpdated;
    
    public FileTransferService(JwtService jwtService, IJSRuntime jsRuntime)
    {
        _jwtService = jwtService;
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeModuleAsync()
    {
        _objectReference ??= DotNetObjectReference.Create(this);

        if (_jsModule is null)
        {
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/FileTransfer.js");
            await _jsModule.InvokeVoidAsync("initialize", _objectReference);
        }
    }

    public async Task<bool> UploadFileAsync()
    {
        if (_uploadInProgress) return false;
        
        await _jsModule!.InvokeVoidAsync("uploadSelectedFile");
        
        return true;
    }

    [JSInvokable]
    public string GetJwt() => _jwtService.Token;

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

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }
        
        _objectReference?.Dispose();
    }
}