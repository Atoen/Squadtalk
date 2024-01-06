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

    public EmbedService(ImagePreviewGenerator previewGenerator)
    {
        _previewGenerator = previewGenerator;
    }
    
    public async Task<Embed> CreateEmbedAsync(ITusFile file, CancellationToken cancellationToken)
    {
        var metadata = await file.GetMetadataAsync(cancellationToken);

        var filename = metadata.GetString(FileData.FileName);
        var filesize = metadata.GetString(FileData.FileSize);
        var contentType = metadata.GetString(FileData.ContentType);

        var url = CreateDownloadUrl(file.Id, filename);
        
        var embed = CreateEmbed(filename, filesize, url, EmbedType.File);

        if (contentType.StartsWith("image/"))
        {
            return await AddImageDataAsync(embed, file, metadata, cancellationToken);
        }

        if (contentType.StartsWith("video/"))
        {
            
        }

        return embed;
    }

    private async Task<Embed> AddImageDataAsync(Embed embed, ITusFile file, Dictionary<string, Metadata> metadata,
        CancellationToken cancellationToken)
    {
        embed.Type = EmbedType.Image;
        
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

        if (!_previewGenerator.ShouldCreatePreview(imageSize))
        {
            return embed;
        }
        
        var previewData = await _previewGenerator.CreatePreviewAsync(file, cancellationToken);
        var (id, name, size) = previewData;

        data[FileData.PreviewUrl] = CreateDownloadUrl(id, name);
        data[FileData.ImageWidth] = size.Width.ToString();
        data[FileData.ImageHeight] = size.Height.ToString();

        return embed;
    }

    private Embed CreateEmbed(string filename, string filesize, string url, EmbedType type)
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
    
    // private async Task<Embed> CreateImageEmbedAsync(I)

    private string CreateDownloadUrl(string fileId, string filename)
    {
        return $"http://localhost:1235/api/files/{fileId}/{filename}";
    }
}