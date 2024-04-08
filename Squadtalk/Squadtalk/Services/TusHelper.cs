using System.Text;
using Shared.Extensions;
using Squadtalk.Extensions;
using tusdotnet.Stores;

namespace Squadtalk.Services;

public class TusHelper
{
    private readonly ILogger<TusHelper> _logger;
    public string StorePath { get; }

    public TusDiskStore DiskStore { get; }
        
    public TusHelper(IConfiguration configuration, ILogger<TusHelper> logger)
    {
        _logger = logger;

        StorePath = configuration.GetString("Tus:Path");
        DiskStore = new TusDiskStore(StorePath);
    }

    public async Task<string?> CreateFileAsync(Stream stream, long fileSize, string metadata, CancellationToken cancellationToken)
    {
        try
        {
            var fileId = await DiskStore.CreateFileAsync(fileSize, metadata, cancellationToken);

            await DiskStore.SetUploadLengthAsync(fileId, fileSize, cancellationToken);
            await DiskStore.AppendDataAsync(fileId, stream, cancellationToken);

            return fileId;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create file");
            return null;
        }
    }

    public static string FormatMetadata(Dictionary<string, string> metadata)
    {
        var builder = new StringBuilder();
        
        foreach (var (key, value) in metadata)
        {
            var base64 = value.ToBase64();

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