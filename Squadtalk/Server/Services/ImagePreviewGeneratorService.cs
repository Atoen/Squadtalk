﻿using SixLabors.ImageSharp.Formats.Png;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;

namespace Squadtalk.Server.Services;

public class ImagePreviewGeneratorService : IImagePreviewGenerator
{
    private readonly TusDiskStore _tusStore;

    public ImagePreviewGeneratorService(TusDiskStoreHelper diskStoreHelper)
    {
        _tusStore = diskStoreHelper.Store;
    }

    public int MaxWidth { get; set; } = 700;
    public int MaxHeight { get; set; } = 500;

    public bool ShouldResize(int width, int height)
    {
        return width > MaxWidth || height > MaxHeight;
    }

    public async Task<(string id, int width, int height)> CreateImagePreviewAsync(ITusFile imageFile,
        CancellationToken cancellationToken)
    {
        await using var imageData = await imageFile.GetContentAsync(cancellationToken).ConfigureAwait(false);
        var metadata = await imageFile.GetMetadataAsync(cancellationToken).ConfigureAwait(false);
        using var image = await Image.LoadAsync(imageData, cancellationToken).ConfigureAwait(false);

        var (targetWidth, targetHeight) = GetResizedDimensions(image);

        image.Mutate(x => x.Resize(targetWidth, targetHeight, KnownResamplers.NearestNeighbor));

        using var stream = new MemoryStream();
        await image.SaveAsync(stream, new PngEncoder(), cancellationToken).ConfigureAwait(false);

        stream.Seek(0, SeekOrigin.Begin);

        var formattedMetadata = FormatMetadata(metadata);
        var id = await _tusStore.CreateFileAsync(stream.Length, formattedMetadata, cancellationToken)
            .ConfigureAwait(false);

        await _tusStore.SetUploadLengthAsync(id, stream.Length, cancellationToken).ConfigureAwait(false);
        await _tusStore.AppendDataAsync(id, stream, cancellationToken).ConfigureAwait(false);

        return (id, image.Width, image.Height);
    }

    public (int width, int height) GetResizedDimensions(Image image)
    {
        var (width, height) = (image.Width, image.Height);

        if (width > MaxWidth && height > MaxHeight)
        {
            return CalculateDimensions(width, height);
        }

        return width > MaxWidth ? (MaxWidth, 0) : (0, MaxHeight);
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