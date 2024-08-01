using Shared;
using Shared.Data.TypedIds;
using Shared.Enums;
using SixLabors.ImageSharp;
using Squadtalk.Data.Entities;
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
    
    public async Task<Embed> CreateFileEmbedAsync(ITusFile file, ChannelId channelId, CancellationToken cancellationToken)
    {
        var metadata = await file.GetMetadataAsync(cancellationToken);

        var filename = metadata.GetString(EmbedData.FileName);
        var filesize = metadata.GetString(EmbedData.FileSize);
        var contentType = metadata.GetString(EmbedData.ContentType);

        var url = CreateDownloadUrl(channelId, file.Id, filename);
        var embed = CreateFileEmbed(filename, filesize, url, EmbedType.File);

        if (contentType.StartsWith(ImageMime))
        {
            await AddImageDataAsync(embed, file, channelId, metadata, cancellationToken);
        }

        else if (contentType.StartsWith(VideoMime))
        {
            
        }

        return embed;
    }

    private async Task AddImageDataAsync(Embed embed, ITusFile file, ChannelId channelId, Dictionary<string, Metadata> metadata,
        CancellationToken cancellationToken)
    {
        var width = metadata.GetString(EmbedData.ImageWidth);
        var height = metadata.GetString(EmbedData.ImageHeight);

        var data = embed.Data;
        data[EmbedData.ImageWidth] = width;
        data[EmbedData.ImageHeight] = height;
        data[EmbedData.PreviewUrl] = data[EmbedData.Url];

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

        data[EmbedData.PreviewUrl] = CreateDownloadUrl(channelId,  id, name);
        data[EmbedData.ImageWidth] = size.Width.ToString();
        data[EmbedData.ImageHeight] = size.Height.ToString();
    }

    private static Embed CreateFileEmbed(string filename, string filesize, string url, EmbedType type)
    {
        return new Embed
        {
            Type = type,
            Data = new Dictionary<string, string>
            {
                {EmbedData.Url, url},
                {EmbedData.FileName, filename},
                {EmbedData.FileSize, filesize}
            }
        };
    }

    private string CreateDownloadUrl(ChannelId channelId, string fileId, string filename)
    {
        return $"{_urlBasePath}/api/files/{channelId.Value}/{fileId}/{filename}";
    }
}
