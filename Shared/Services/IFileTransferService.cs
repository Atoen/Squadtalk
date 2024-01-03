using Shared.Communication;
using Shared.Models;

namespace Shared.Services;

public interface IFileTransferService
{
    event Action<FileModel>? FileSelected;
    event Action? SelectionCleared;

    event Action<FileModel, TextChannel>? UploadStarted; 
    event Action<string?>? UploadEnded;

    event Action<double>? UploadProgressUpdated;
    event Action<string>? UploadSpeedUpdated;

    event Action? StateChanged;

    bool IsFileAvailable => SelectedFile is not null;
    
    FileModel? SelectedFile { get; }

    TextChannel? UploadChannel { get; }
    
    List<FileModel> UploadQueue { get; }

    bool IsQueueEmpty => UploadQueue.Count == 0;
    
    Task InitializeAsync();

    Task UploadFileAsync(TextChannel channel);

    Task CancelUploadAsync();

    Task RemoveSelectedFileAsync();

    void RemoveFileFromQueue(FileModel file);
}