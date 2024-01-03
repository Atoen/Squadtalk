using System.Runtime.InteropServices.JavaScript;
using Shared.Models;

namespace Squadtalk.Client.Services;

public partial class FileTransferService
{
    private static FileTransferService? _instance;

    private const string Module = "Files";

    [JSImport("removeSelectedFile", Module)]
    internal static partial void RemoveSelectedFile();

    [JSExport]
    internal static void FileSelectedCallback(string filename, [JSMarshalAs<JSType.Number>] long filesize)
    {
        var file = FileModel.Create(filename, filesize);
        _instance?.SelectFile(file);
    }

    [JSExport]
    internal static void UploadEndedCallback(string? error)
    {
        _instance?.UploadEnd(error);
    }

    [JSImport("uploadSelectedFile", Module)]
    [return: JSMarshalAs<JSType.Promise<JSType.Void>>]
    internal static partial Task UploadSelectedFile(string channelId);

    [JSImport("initialize", Module)]
    internal static partial void InitializeModule(string endpoint);

    [JSImport("cancelUpload", Module)]
    internal static partial void CancelUpload();

    [JSImport("removeFromQueue", Module)]
    private static partial void RemoveFromQueue(int filename);

    [JSExport]
    private static void FileAddedToQueueCallback(string filename, [JSMarshalAs<JSType.Number>] long filesize)
    {
        var file = FileModel.Create(filename, filesize);
        _instance?.FileAddedToQueue(file);
    }
    
    [JSExport]
    private static void UploadStartedCallback(string filename, [JSMarshalAs<JSType.Number>] long filesize)
    {
        var file = FileModel.Create(filename, filesize);
        _instance?.UploadStarted2(file);
    }
}