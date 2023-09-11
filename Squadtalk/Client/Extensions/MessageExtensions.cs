using System.Globalization;
using System.Runtime.CompilerServices;
using Squadtalk.Client.Models;
using Squadtalk.Shared;

namespace Squadtalk.Client.Extensions;

public static class MessageExtensions
{
    public static MessageModel ToModel(this MessageDto message)
    {
        var model = new MessageModel
        {
            Author = message.Author.Username,
            Timestamp = message.Timestamp,
            Content = message.Content
        };

        if (message.Embed is { Type: EmbedType.Gif })
        {
            model.Embed = new EmbedModel
            {
                Type = message.Embed.Type,
                Data =
                {
                    { "Source", message.Embed["Uri"] }
                }
            };
        }

        else if (message.Embed is { Type: EmbedType.Image })
        {
            model.Embed = new EmbedModel
            {
                Type = message.Embed.Type,
                Data =
                {
                    { "Source", message.Embed["Uri"] },
                    { "Preview", message.Embed["Preview"] }
                }
            };
            
            if (message.Embed.Data.TryGetValue("Width", out var width) &&
                message.Embed.Data.TryGetValue("Height", out var height))
            {
                model.Embed.Data["Width"] = width;
                model.Embed.Data["Height"] = height;
            }
            else
            {
                model.Embed.Data["Width"] = "auto";
                model.Embed.Data["Height"] = "auto";
            }
        }

        else if (message.Embed is { Type: EmbedType.File })
        {
            var filename = message.Embed["Filename"];
            var fileType = GetFileType(filename).ToString();

            model.Embed = new EmbedModel
            {
                Type = EmbedType.File,
                Data =
                {
                    { "Source", message.Embed["Uri"] },
                    { "Filename", filename },
                    { "FileSize", message.Embed["FileSize"] },
                    { "FileType", fileType }
                }
            };
        }

        return model;
    }

    private const string Pdf = "pdf";
    private const string Text = "txt";
    private static readonly List<string> Archive = new() { "zip", "rar", "7z" };
    private static readonly List<string> Code = new() { "cs", "java", "cpp", "py", "js", "c" };
    private static readonly List<string> Video = new() { "mp4", "avi", "mov", "mkv" };
    private static readonly List<string> Music = new() { "mp3", "wav", "flac", "mp2", "ogg" };
    private static readonly List<string> Image = new() { "jpeg", "jpg", "png", "gif", "raw" };

    public static FileType GetFileType(string filename)
    {
        var tokens = filename.Split('.');

        if (tokens.Length < 2) return FileType.Default;
        var extension = tokens[^1].ToLower();

        if (extension == Pdf) return FileType.Pdf;
        if (extension == Text) return FileType.Text;

        if (Archive.Contains(extension)) return FileType.Archive;
        if (Code.Contains(extension)) return FileType.Code;
        if (Video.Contains(extension)) return FileType.Video;
        if (Music.Contains(extension)) return FileType.Music;
        if (Image.Contains(extension)) return FileType.Image;

        Console.WriteLine($"returning default for {filename} - extension: {extension}");
        
        return FileType.Default;
    }

    public static string ConvertToHumanReadableSize(string value, [CallerMemberName] string? name = null)
    {
        const long bytesPerKiloByte = 1024;
        const long bytesPerMegaByte = 1024 * bytesPerKiloByte;
        const long bytesPerGigaByte = 1024 * bytesPerMegaByte;

        string formattedValue;

        var success = long.TryParse(value, out var bytes);

        if (!success)
        {
            Console.WriteLine($"Couldn't parse value: {value} for {name}");
            
            return value;
        }

        switch (bytes)
        {
            case < bytesPerKiloByte:
            {
                formattedValue = $"{bytes} B";
                break;
            }
            case < bytesPerMegaByte:
            {
                var kiloBytes = (double)bytes / bytesPerKiloByte;
                formattedValue = $"{kiloBytes.ToString("F2", CultureInfo.InvariantCulture)} kB";
                break;
            }
            case < bytesPerGigaByte:
            {
                var megaBytes = (double)bytes / bytesPerMegaByte;
                formattedValue = $"{megaBytes.ToString("F2", CultureInfo.InvariantCulture)} MB";
                break;
            }
            default:
            {
                var gigaBytes = (double)bytes / bytesPerGigaByte;
                formattedValue = $"{gigaBytes.ToString("F2", CultureInfo.InvariantCulture)} GB";
                break;
            }
        }

        return formattedValue;
    }
}

public enum FileType
{
    Default,
    Pdf,
    Text,
    Archive,
    Code,
    Video,
    Music,
    Image
}