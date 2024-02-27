using Shared.Communication;
using Shared.Models;

namespace Shared.Services;

public interface IFileTransferService
{
    event Action<FileModel>? FileSelected;
    event Action? SelectionCleared;

    event Action<FileModel, TextChannel>? UploadStarted; 
    event Action? StateChanged;

    bool IsFileAvailable => SelectedFile is not null;

    int SelectedCount { get; }

    bool SelectedMultiple => SelectedCount > 1;
    
    FileModel? SelectedFile { get; }
    
    FileModel? CurrentlyUploadedFile { get; }

    TextChannel? UploadChannel { get; }
    
    List<FileModel> UploadQueue { get; }

    bool IsQueueEmpty => UploadQueue.Count == 0;
    
    Task InitializeAsync();

    Task UploadFileAsync(TextChannel channel);

    Task CancelUploadAsync();

    Task RemoveSelectedFileAsync();

    Task RemoveFileFromQueueAsync(FileModel file);
}