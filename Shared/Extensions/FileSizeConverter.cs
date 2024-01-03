using System.Globalization;

namespace Shared.Extensions;

public static class FileSizeConverter
{
    public const long BytesPerKiloByte = 1000;
    public const long BytesPerMegaByte = 1000 * BytesPerKiloByte;
    public const long BytesPerGigaByte = 1000 * BytesPerMegaByte;
    
    public static string ConvertToHumanReadableSize(long length)
    {
        static string Format(double num) => num.ToString("F2", CultureInfo.InvariantCulture);

        switch (length)
        {
            case < BytesPerKiloByte:
            {
                return $"{length} B";
            }
            case < BytesPerMegaByte:
            {
                var kiloBytes = (double) length / BytesPerKiloByte;
                return $"{Format(kiloBytes)} kB";
            }
            case < BytesPerGigaByte:
            {
                var megaBytes = (double) length / BytesPerMegaByte;
                return $"{Format(megaBytes)} MB";
            }
            default:
            {
                var gigaBytes = (double) length / BytesPerGigaByte;
                return $"{Format(gigaBytes)} GB";
            }
        }
    }
}