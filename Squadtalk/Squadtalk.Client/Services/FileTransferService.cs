using System.Runtime.InteropServices.JavaScript;
using Shared.Communication;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public sealed partial class FileTransferService : IFileTransferService
{
    private readonly ILogger<FileTransferService> _logger;

    public event Action<FileModel>? FileSelected;
    public event Action? SelectionCleared;
    public event Action<FileModel, TextChannel>? UploadStarted;
    public event Action<string?>? UploadEnded;
    public event Action<double>? UploadProgressUpdated;
    public event Action<string>? UploadSpeedUpdated;
    public event Action? StateChanged;
    
    private bool _uploadInProgress;
    
    public TextChannel? UploadChannel { get; private set; }
    
    public FileModel? SelectedFile { get; private set; }

    public List<FileModel> UploadQueue { get; } = [];
    // [
    //     new FileModel { Name = "File.txtfhwe87guifvgwiyvgwyvgwyvwuey fdwrgfwrgwrgwgwr", Size = "23.43GB" },
    //     new FileModel { Name = "File1.txt", Size = "23.43GB" },
    //     new FileModel { Name = "File2.txt", Size = "23.43GB" }
    // ];

    public FileTransferService(ILogger<FileTransferService> logger)
    {
        _logger = logger;

        if (_instance is not null)
        {
            _logger.LogCritical("Instance not null");
        }
        
        _instance = this;
    }

    public async Task InitializeAsync()
    {
        if (!OperatingSystem.IsBrowser())
        {
            return;
        }

        await JSHost.ImportAsync(Module, "../js/FileTransfer.js");
        
        InitializeModule("http://localhost:1235/Upload");
    }

    public Task UploadFileAsync(TextChannel channel)
    {
        if (SelectedFile is null)
        {
            _logger.LogWarning("Selected file is null");
            return Task.CompletedTask;
        }

        // if (_uploadInProgress)
        // {
        //     ;
        // }
        //
        // UploadStarted?.Invoke(SelectedFile, channel);
        
        SelectedFile = null;
        
        return UploadSelectedFile(channel.Id);
    }

    public Task CancelUploadAsync()
    {
        CancelUpload();
        
        return Task.CompletedTask;
    }

    public Task RemoveSelectedFileAsync()
    {
        RemoveSelectedFile();
        SelectedFile = null;
        
        SelectionCleared?.Invoke();
        
        return Task.CompletedTask;
    }

    public void RemoveFileFromQueue(FileModel file)
    {
        var index = UploadQueue.IndexOf(file);

        if (index == -1)
        {
            _logger.LogWarning("Attempt to remove nonexistent index: {Index}", index);
            return;
        }
        
        UploadQueue.RemoveAt(index);
        RemoveFromQueue(index);

        if (UploadQueue.Count == 0)
        {
            StateChanged?.Invoke();
        }
    }

    private void SelectFile(FileModel file)
    {
        SelectedFile = file;
        FileSelected?.Invoke(SelectedFile);
    }
    
    private void UploadEnd(string? error)
    {
        _uploadInProgress = false;
        UploadEnded?.Invoke(error);
    }

    private void FileAddedToQueue(FileModel file)
    {
        _logger.LogInformation("Adding {File} to queue", file.Name);
        
        UploadQueue.Add(file);
        StateChanged?.Invoke();
    }

    private void UploadStarted2(FileModel file)
    {
        UploadStarted?.Invoke(file, GroupChat.GlobalChat);
    }
}