using System.Text;
using tusdotnet.Stores;

namespace Squadtalk.Services;

public class TusHelper
{
    public string StorePath { get; }

    public TusDiskStore DiskStore { get; }
        
    public TusHelper(IConfiguration configuration)
    {
        var path = configuration["Tus:Path"];
        ArgumentException.ThrowIfNullOrEmpty(path);

        StorePath = path;
        DiskStore = new TusDiskStore(path);
    }

    public async Task<string> CreateFileAsync(Stream stream, long fileSize, string metadata, CancellationToken cancellationToken)
    {
        var fileId = await DiskStore.CreateFileAsync(fileSize, metadata, cancellationToken);

        await DiskStore.SetUploadLengthAsync(fileId, fileSize, cancellationToken);
        await DiskStore.AppendDataAsync(fileId, stream, cancellationToken);

        return fileId;
    }

    public static string FormatMetadata(Dictionary<string, string> metadata)
    {
        var builder = new StringBuilder();
        
        foreach (var (key, value) in metadata)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var base64 = Convert.ToBase64String(bytes);

            if (builder.Length > 0)
            {
                builder.Append(',');
            }

            builder.Append(key);
            builder.Append(' ');
            builder.Append(base64);
        }

        return builder.ToString();
    }
}