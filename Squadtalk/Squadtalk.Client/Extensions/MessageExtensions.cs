using System.Runtime.CompilerServices;
using Shared.DTOs;
using Shared.Models;

namespace Squadtalk.Client.Extensions
{
    public static class MessageExtensions
    {
        private const string Pdf = "pdf";
        private const string Text = "txt";

        private static readonly List<string> Archive = new() { "zip", "rar", "7z" };
        private static readonly List<string> Code = new() { "cs", "java", "cpp", "py", "js", "c" };
        private static readonly List<string> Video = new() { "mp4", "avi", "mov", "mkv" };
        private static readonly List<string> Music = new() { "mp3", "wav", "flac", "mp2", "ogg" };
        private static readonly List<string> Image = new() { "jpeg", "jpg", "png", "gif", "raw" };

        public static MessageModel ToModel(this MessageDto message)
        {
            var model = new MessageModel
            {
                Author = message.Author.Username,
                Timestamp = message.Timestamp,
                Content = message.Content
            };

            // if (TryCreateEmbedModel(message.Embed, out var embedModel))
            // {
            //     model.Embed = embedModel;
            // }

            return model;
        }

        // private static bool TryCreateEmbedModel(EmbedDto? embedDto, out EmbedModel? embedModel)
        // {
        //     embedModel = null;
        //
        //     if (embedDto is null)
        //     {
        //         return false;
        //     }
        //
        //     embedModel = new EmbedModel
        //     {
        //         Type = embedDto.Type
        //     };
        //
        //     switch (embedDto.Type)
        //     {
        //         case EmbedType.Gif:
        //             embedModel.Data["Source"] = embedDto["Uri"];
        //             break;
        //         case EmbedType.Image:
        //             embedModel.Data["Source"] = embedDto["Uri"];
        //             embedModel.Data["Preview"] = embedDto["Preview"];
        //             SetWidthAndHeight(embedDto, embedModel);
        //             break;
        //         case EmbedType.File:
        //             var filename = embedDto["Filename"];
        //             var fileType = GetFileType(filename).ToString();
        //
        //             embedModel.Data["Source"] = embedDto["Uri"];
        //             embedModel.Data["Filename"] = filename;
        //             embedModel.Data["FileSize"] = embedDto["FileSize"];
        //             embedModel.Data["FileType"] = fileType;
        //             break;
        //     }
        //
        //     return true;
        // }

        // private static void SetWidthAndHeight(EmbedDto embedDto, EmbedModel embedModel)
        // {
        //     if (embedDto.Data.TryGetValue("Width", out var width) &&
        //         embedDto.Data.TryGetValue("Height", out var height))
        //     {
        //         embedModel.Data["Width"] = width;
        //         embedModel.Data["Height"] = height;
        //     }
        //     else
        //     {
        //         embedModel.Data["Width"] = "auto";
        //         embedModel.Data["Height"] = "auto";
        //     }
        // }

        public static EmbedFileType GetFileType(string filename)
        {
            var tokens = filename.Split('.');

            if (tokens.Length < 2) return EmbedFileType.Default;
            var extension = tokens[^1].ToLower();

            if (extension == Pdf) return EmbedFileType.Pdf;
            if (extension == Text) return EmbedFileType.Text;

            if (Archive.Contains(extension)) return EmbedFileType.Archive;
            if (Code.Contains(extension)) return EmbedFileType.Code;
            if (Video.Contains(extension)) return EmbedFileType.Video;
            if (Music.Contains(extension)) return EmbedFileType.Music;
            if (Image.Contains(extension)) return EmbedFileType.Image;

            return EmbedFileType.Default;
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

            formattedValue = bytes switch
            {
                < bytesPerKiloByte => $"{bytes} B",
                < bytesPerMegaByte => $"{(double)bytes / bytesPerKiloByte:F2} kB",
                < bytesPerGigaByte => $"{(double)bytes / bytesPerMegaByte:F2} MB",
                _ => $"{(double)bytes / bytesPerGigaByte:F2} GB"
            };

            return formattedValue;
        }
    }

    public enum EmbedFileType
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
}
