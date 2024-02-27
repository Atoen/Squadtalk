using Shared.Enums;
using Shared.Extensions;

namespace Shared.Models;

public class FileModel
{
    public required string Name { get; init; }
    public required string Size { get; init; }

    public FileType Type { get; init; }

    public static FileModel Create(string name, long size)
    {
        var humanReadableSize = FileSizeConverter.ConvertToHumanReadableSize(size);

        return new FileModel
        {
            Name = name,
            Size = humanReadableSize,
            Type = GetType(name)
        };
    }
    
    private const string Pdf = "pdf";
    private const string Text = "txt";
    private static readonly List<string> Archive = ["zip", "rar", "7z"];
    private static readonly List<string> Code = ["cs", "java", "cpp", "py", "js", "c"];
    private static readonly List<string> Video = ["mp4", "avi", "mov", "mkv"];
    private static readonly List<string> Music = ["mp3", "wav", "flac", "mp2", "ogg"];
    private static readonly List<string> Image = ["jpeg", "jpg", "png", "gif", "raw"];

    public static FileType GetType(string name)
    {
        var tokens = name.Split('.');

        if (tokens.Length < 2) return FileType.Default;
        var extension = tokens[^1].ToLower();

        if (extension == Pdf) return FileType.Pdf;
        if (extension == Text) return FileType.Text;

        if (Archive.Contains(extension)) return FileType.Archive;
        if (Code.Contains(extension)) return FileType.Code;
        if (Video.Contains(extension)) return FileType.Video;
        if (Music.Contains(extension)) return FileType.Music;
        if (Image.Contains(extension)) return FileType.Image;

        return FileType.Default;
    }
}