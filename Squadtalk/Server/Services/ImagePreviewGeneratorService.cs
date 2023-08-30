using System.Diagnostics;
using SixLabors.ImageSharp.Formats.Png;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;

namespace Squadtalk.Server.Services;

public class ImagePreviewGeneratorService
{
    private readonly ILogger<ImagePreviewGeneratorService> _logger;
    private readonly TusDiskStore _tusStore;

    public static int MaxWidth { get; set; } = 700;
    public static int MaxHeight { get; set; } = 500;

    [Flags]
    public enum ResizingDimension
    {
        None,
        Width,
        Height,
        Both,
    }

    public ImagePreviewGeneratorService(TusDiskStoreHelper diskStoreHelper, ILogger<ImagePreviewGeneratorService> logger)
    {
        _logger = logger;
        _tusStore = new TusDiskStore(diskStoreHelper.Path);
    }

    public async Task<(string id, int width, int height)> CreateImagePreviewAsync(ITusFile imageFile, CancellationToken cancellationToken)
    {
        var start = Stopwatch.GetTimestamp();
        
        var imageData = await imageFile.GetContentAsync(cancellationToken);
        var metadata = await imageFile.GetMetadataAsync(cancellationToken);
        using var image = await Image.LoadAsync(imageData, cancellationToken);

        _logger.LogInformation("Original image size: {Size}", image.Size);

        var loaded = Stopwatch.GetTimestamp();
        _logger.LogInformation("Loaded image in {Time}", Stopwatch.GetElapsedTime(start, loaded));
        
        var (targetWidth, targetHeight) = GetResizedDimensions(image);

        image.Mutate(x => x.Resize(targetWidth, targetHeight, KnownResamplers.NearestNeighbor));

        var resized = Stopwatch.GetTimestamp();
        _logger.LogInformation("Resized image in {Time}", Stopwatch.GetElapsedTime(loaded, resized));
        _logger.LogInformation("Resized image size: {Size}", image.Size);

        using var stream = new MemoryStream();
        await image.SaveAsync(stream, new PngEncoder(), cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);

        var formattedMetadata = FormatMetadata(metadata);
        var id = await _tusStore.CreateFileAsync(stream.Length, formattedMetadata, cancellationToken);
        
        await _tusStore.SetUploadLengthAsync(id, stream.Length, cancellationToken);
        await _tusStore.AppendDataAsync(id, stream, cancellationToken);

        var saved = Stopwatch.GetTimestamp();
        _logger.LogInformation("Saved image preview in {Time}", Stopwatch.GetElapsedTime(resized, saved));
        _logger.LogInformation("Created preview in {Time}", Stopwatch.GetElapsedTime(start, saved));
        
        return (id, image.Width, image.Height);
    }

    private string FormatMetadata(Dictionary<string, Metadata> metadata)
    {
        var filename = metadata["filename"].GetBytes();
        var filetype = "image/png"u8;
        var filesize = metadata["filesize"].GetBytes();

        var name64 = Convert.ToBase64String(filename);
        var type64 = Convert.ToBase64String(filetype);
        var size64 = Convert.ToBase64String(filesize);

        return $"filename {name64},filetype {type64},filesize {size64}";
    }

    private (int width, int height) GetResizedDimensions(Image image)
    {
        var (width, height) = (image.Width, image.Height);

        if (width > MaxWidth && height > MaxHeight)
        {
            return CalculateDimensions(width, height);
        }
        
        return width > MaxWidth ? (MaxWidth, 0) : (0, MaxHeight);
    }

    private (int width, int height) CalculateDimensions(int width, int height)
    {
        var widthScale = (double) MaxWidth / width;
        var heightScale = (double) MaxHeight / height;
        
        var scale = Math.Min(widthScale, heightScale);

        var newWidth = (int) (width * scale);
        var newHeight = (int) (height * scale);

        return (newWidth, newHeight);
    }
}