using Shared;
using Shared.Enums;
using SixLabors.ImageSharp;
using Squadtalk.Data;
using Squadtalk.Extensions;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace Squadtalk.Services;

public class EmbedService
{
    private readonly ImagePreviewGenerator _previewGenerator;

    private const string ImageMime = "image/";
    private const string VideoMime = "video/";
    private readonly string _urlBasePath;

    public EmbedService(ImagePreviewGenerator previewGenerator, IConfiguration configuration)
    {
        _previewGenerator = previewGenerator;
        _urlBasePath = configuration.GetString("Rest:BasePath");
    }
    
    public async Task<Embed> CreateFileEmbedAsync(ITusFile file, CancellationToken cancellationToken)
    {
        var metadata = await file.GetMetadataAsync(cancellationToken);

        var filename = metadata.GetString(FileData.FileName);
        var filesize = metadata.GetString(FileData.FileSize);
        var contentType = metadata.GetString(FileData.ContentType);

        var url = CreateDownloadUrl(file.Id, filename);
        var embed = CreateFileEmbed(filename, filesize, url, EmbedType.File);

        if (contentType.StartsWith(ImageMime))
        {
            await AddImageDataAsync(embed, file, metadata, cancellationToken);
        }

        else if (contentType.StartsWith(VideoMime))
        {
            
        }

        return embed;
    }

    private async Task AddImageDataAsync(Embed embed, ITusFile file, Dictionary<string, Metadata> metadata,
        CancellationToken cancellationToken)
    {
        var width = metadata.GetString(FileData.ImageWidth);
        var height = metadata.GetString(FileData.ImageHeight);

        var data = embed.Data;
        data[FileData.ImageWidth] = width;
        data[FileData.ImageHeight] = height;
        data[FileData.PreviewUrl] = data[FileData.Url];

        var imageSize = new Size
        {
            Width = int.Parse(width),
            Height = int.Parse(height)
        };
        
        embed.Type = EmbedType.Image;

        if (!_previewGenerator.ShouldCreatePreview(imageSize)) return;
        
        var previewData = await _previewGenerator.CreatePreviewAsync(file, cancellationToken);
        if (previewData is null)
        {
            embed.Type = EmbedType.File;
            return;
        }
        
        var (id, name, size) = previewData;

        data[FileData.PreviewUrl] = CreateDownloadUrl(id, name);
        data[FileData.ImageWidth] = size.Width.ToString();
        data[FileData.ImageHeight] = size.Height.ToString();
    }

    private static Embed CreateFileEmbed(string filename, string filesize, string url, EmbedType type)
    {
        return new Embed
        {
            Type = type,
            Data = new Dictionary<string, string>
            {
                {FileData.Url, url},
                {FileData.FileName, filename},
                {FileData.FileSize, filesize}
            }
        };
    }

    private string CreateDownloadUrl(string fileId, string filename)
    {
        return $"{_urlBasePath}/api/files/{fileId}/{filename}";
    }
}