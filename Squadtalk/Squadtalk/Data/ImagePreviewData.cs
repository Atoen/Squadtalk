using SixLabors.ImageSharp;

namespace Squadtalk.Data;

public record ImagePreviewData(string FileId, string FileName, Size PreviewSize);