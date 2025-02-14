using SixLabors.ImageSharp;
using Squadtalk.Data.TypedIds;

namespace Squadtalk.Data;

public record ImagePreviewData(TusFileId FileId, string FileName, Size PreviewSize);