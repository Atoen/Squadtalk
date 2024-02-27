using tusdotnet.Interfaces;

namespace Squadtalk.Server.Services;

public interface IImagePreviewGenerator
{
    int MaxWidth { get; }
    int MaxHeight { get; }

    Task<(string id, int width, int height)> CreateImagePreviewAsync(ITusFile imageFile, CancellationToken cancellationToken);

    (int width, int height) GetResizedDimensions(Image image);

    bool ShouldResize(int width, int height);
}