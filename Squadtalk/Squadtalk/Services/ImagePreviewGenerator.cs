using Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Squadtalk.Extensions;
using tusdotnet.Interfaces;

namespace Squadtalk.Services;

public class ImagePreviewGenerator
{
    private readonly TusHelper _tusHelper;
    private readonly PngEncoder _pngEncoder = new();

    public ImagePreviewGenerator(TusHelper tusHelper)
    {
        _tusHelper = tusHelper;
    }

    public Size MaxPreviewSize { get; set; } = new(700, 500);

    public bool ShouldCreatePreview(Size size)
    {
        return size.Width > MaxPreviewSize.Width || size.Height > MaxPreviewSize.Height;
    }

    public async Task<(string fileId, string filename, Size Size)> CreatePreviewAsync(ITusFile file, CancellationToken cancellationToken)
    {
        await using var fileContent = await file.GetContentAsync(cancellationToken);
        var originalMetadata = await file.GetMetadataAsync(cancellationToken);

        using var image = await Image.LoadAsync(fileContent, cancellationToken);
        var targetSize = GetResizedDimensions(image.Size);
        
        image.Mutate(x => x.Resize(targetSize.Width, targetSize.Height, KnownResamplers.Box));

        using var stream = new MemoryStream();
        await image.SaveAsync(stream, _pngEncoder, cancellationToken);

        var fileSize = stream.Position;
        stream.Seek(0, SeekOrigin.Begin);

        var originalName = originalMetadata.GetString(FileData.FileName);
        var previewName = "preview_" + originalName;

        var previewMetadata = new Dictionary<string, string>
        {
            { FileData.FileName,  previewName},
            { FileData.FileSize, stream.Position.ToString() },
            { FileData.ContentType, "image/png" },
            { FileData.ImageWidth, image.Width.ToString() },
            { FileData.ImageHeight, image.Height.ToString() }
        };
        
        var formattedMetadata = TusHelper.FormatMetadata(previewMetadata);
        var fileId = await _tusHelper.CreateFileAsync(stream, fileSize, formattedMetadata, cancellationToken);

        return (fileId, previewName, image.Size);
    }

    private Size GetResizedDimensions(Size size)
    {
        if (size.Width > MaxPreviewSize.Width && size.Height > MaxPreviewSize.Height)
        {
            return CalculateDimensions(size);
        }

        return size.Width > MaxPreviewSize.Width
            ? new Size { Width = MaxPreviewSize.Width, Height = 0 }
            : new Size { Width = 0, Height = MaxPreviewSize.Height };
    }

    private Size CalculateDimensions(Size size)
    {
        var widthScale = (double) MaxPreviewSize.Width / size.Width;
        var heightScale = (double) MaxPreviewSize.Height / size.Height;

        var scale = Math.Min(widthScale, heightScale);

        return new Size
        {
            Width = (int) (size.Width * scale),
            Height = (int) (size.Height * scale)
        };
    }
}