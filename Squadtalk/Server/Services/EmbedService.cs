using System.Text;
using Squadtalk.Server.Models;
using Squadtalk.Shared;
using tusdotnet.Interfaces;

namespace Squadtalk.Server.Services;

public class EmbedService
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png" };
    private readonly IImagePreviewGenerator _previewGenerator;
    private string? _requestHost;

    // Service is transient
    private string? _requestScheme;

    public EmbedService(IImagePreviewGenerator previewGenerator)
    {
        _previewGenerator = previewGenerator;
    }

    public async Task<Embed> CreateEmbedAsync(ITusFile file, string requestScheme, string requestHost,
        CancellationToken cancellationToken)
    {
        _requestScheme = requestScheme;
        _requestHost = requestHost;

        var metadata = await file.GetMetadataAsync(cancellationToken);

        var filename = metadata["filename"].GetString(Encoding.UTF8);
        var length = metadata["filesize"].GetString(Encoding.UTF8);
        var uri = CreateUri(file.Id, requestScheme, requestHost);

        if (!HasImageExtension(filename))
        {
            return new Embed
            {
                Type = EmbedType.File,
                Data = new Dictionary<string, string>
                {
                    { "Uri", uri },
                    { "Filename", filename },
                    { "FileSize", length }
                }
            };
        }

        var width = metadata["width"].GetString(Encoding.UTF8);
        var height = metadata["height"].GetString(Encoding.UTF8);

        return await CreateImageEmbed(file, uri, width, height, cancellationToken);
    }

    private async Task<Embed> CreateImageEmbed(ITusFile file, string uri, string width, string height,
        CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, string>
        {
            { "Uri", uri },
            { "Width", width },
            { "Height", height },
            { "Preview", uri }
        };

        var (widthInt, heightInt) = (int.Parse(width), int.Parse(height));

        if (_previewGenerator.ShouldResize(widthInt, heightInt))
        {
            var (previewId, previewWidth, previewHeight) = await _previewGenerator.CreateImagePreviewAsync(file, cancellationToken);

            data["Preview"] = CreateUri(previewId, _requestScheme, _requestHost);
            data["Width"] = previewWidth.ToString();
            data["Height"] = previewHeight.ToString();
        }

        return new Embed
        {
            Type = EmbedType.Image,
            Data = data
        };
    }

    private bool HasImageExtension(string filename)
    {
        return ImageExtensions.Any(filename.EndsWith);
    }

    private string CreateUri(string id, string? requestScheme, string? requestHost)
    {
        return $"{requestScheme}://{requestHost}/api/File?id={id}";
    }
}