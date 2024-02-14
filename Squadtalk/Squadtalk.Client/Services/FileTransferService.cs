using BlazorBootstrap;
using Microsoft.JSInterop;
using Shared.Communication;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public sealed class FileTransferService : IFileTransferService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<FileTransferService> _logger;
    private readonly ToastService _toastService;
    private readonly DotNetObjectReference<FileTransferService> _dotNetObject;
    private IJSObjectReference? _jsModule;
    
    public event Action<FileModel>? FileSelected;
    public event Action? SelectionCleared;
    public event Action<FileModel, TextChannel>? UploadStarted;
    public event Action? StateChanged;

    public int SelectedCount { get; private set; }
    public FileModel? SelectedFile { get; private set; }
    public FileModel? CurrentlyUploadedFile { get; private set; }
    public TextChannel? UploadChannel { get; private set; }
    public List<FileModel> UploadQueue { get; } = [];

    public FileTransferService(IJSRuntime jsRuntime, ILogger<FileTransferService> logger, ToastService toastService)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _toastService = toastService;
        _dotNetObject = DotNetObjectReference.Create(this);
    }
    
    public async Task InitializeAsync()
    {
        _jsModule ??= await _jsRuntime
            .InvokeAsync<IJSObjectReference>("import", "../js/FileTransfer.js");

        await _jsModule.InvokeVoidAsync("initialize", _dotNetObject, "127.0.0.1:1235/Upload");
    }

    public Task UploadFileAsync(TextChannel channel)
    {
        if (SelectedFile is null)
        {
            _logger.LogWarning("Selected file is null");
            return Task.CompletedTask;
        }

        SelectedFile = null;
        UploadChannel = channel;

        return _jsModule!.InvokeVoidAsync("uploadSelectedFiles", channel.Id, channel.Name).AsTask();
    }

    public Task CancelUploadAsync()
    {
        if (UploadQueue.Count == 0)
        {
            UploadChannel = null;
        }
        
        return _jsModule!.InvokeVoidAsync("cancelUpload").AsTask();
    }

    public async Task RemoveSelectedFileAsync()
    {
        await _jsModule!.InvokeVoidAsync("removeSelectedFile");
        
        SelectedFile = null;
        SelectionCleared?.Invoke();
    }

    public async Task RemoveFileFromQueueAsync(FileModel file)
    {
        var index = UploadQueue.IndexOf(file);
        if (index == -1)
        {
            _logger.LogWarning("Attempt to remove nonexistent index: {Index}", index);
            return;
        }

        await _jsModule!.InvokeVoidAsync("removeFromQueue", index);
        UploadQueue.RemoveAt(index);
        
        if (UploadQueue.Count == 0)
        {
            StateChanged?.Invoke();
        }
    }

    [JSInvokable]
    public void InvalidUploadCallback(string reason)
    {
        var toast = new ToastMessage(ToastType.Warning, reason);
        _toastService.Notify(toast);
    }

    [JSInvokable]
    public void FileSelectedCallback(string filename, long filesize, int count)
    {
        SelectedCount = count;
        SelectedFile = FileModel.Create(filename, filesize);
        FileSelected?.Invoke(SelectedFile);
    }

    [JSInvokable]
    public void FileAddedToQueueCallback(string filename, long filesize)
    {
        var file = FileModel.Create(filename, filesize);
        UploadQueue.Add(file);
        
        _logger.LogInformation("Added {FileName} to queue", file.Name);
        
        StateChanged?.Invoke();
    }

    [JSInvokable]
    public void UploadStartedCallback(string filename, long filesize)
    {
        CurrentlyUploadedFile = FileModel.Create(filename, filesize);
        UploadStarted?.Invoke(CurrentlyUploadedFile, GroupChat.GlobalChat);
    }

    [JSInvokable]
    public void UploadingFileFromQueueCallback()
    {
        UploadQueue.RemoveAt(0);
        StateChanged?.Invoke();
    }

    [JSInvokable]
    public void UploadQueueFinishedCallback()
    {
        UploadChannel = null;
    }

    public ValueTask DisposeAsync()
    {
        _dotNetObject.Dispose();
        return _jsModule.TryDisposeAsync();
    }
}