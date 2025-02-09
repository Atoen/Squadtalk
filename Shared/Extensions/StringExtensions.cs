using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shared.Extensions;

public static class StringExtensions
{
    private const int MaxSpanLength = 128;

    public static string Format(this string text, object? arg0)
    {
        return string.Format(text, arg0);
    }

    public static string Format(this string text, object? arg0, object? arg1)
    {
        return string.Format(text, arg0, arg1);
    }

    public static string Format(this string text, object? arg0, object? arg1, object? arg2)
    {
        return string.Format(text, arg0, arg1, arg2);
    }

    public static string Format(this string text, params ReadOnlySpan<object?> args)
    {
        return string.Format(text, args);
    }

    public static string ToBase64(this string text, bool urlEncode = false)
    {
        ArgumentNullException.ThrowIfNull(text);

        var bytes = Encoding.UTF8.GetBytes(text);
        var encoded = Convert.ToBase64String(bytes);

        if (!urlEncode)
        {
            return encoded;
        }

        var sourceSpan = encoded.AsSpan();
        var destinationSpan = encoded.Length <= MaxSpanLength
            ? stackalloc char[encoded.Length]
            : new char[encoded.Length];

        sourceSpan.Replace(destinationSpan, '/', '_');
        destinationSpan.Replace('+', '-');

        return destinationSpan.ToString();
    }

    public static string FromBase64(this string encoded, bool urlEncoded = false)
    {
        ArgumentNullException.ThrowIfNull(encoded);

        if (urlEncoded)
        {
            var sourceSpan = encoded.AsSpan();
            var destinationSpan = encoded.Length <= MaxSpanLength
                ? stackalloc char[encoded.Length]
                : new char[encoded.Length];

            sourceSpan.Replace(destinationSpan, '_', '/');
            destinationSpan.Replace('-', '+');

            return Encoding.UTF8.GetString(Convert.FromBase64String(destinationSpan.ToString()));
        }

        var bytes = Convert.FromBase64String(encoded);
        var text = Encoding.UTF8.GetString(bytes);

        return text;
    }

    public static bool TryFromBase64(this string encoded, [NotNullWhen(true)] out string? text, bool urlEncoded = false)
    {
        ArgumentNullException.ThrowIfNull(encoded);

        if (urlEncoded)
        {
            var sourceSpan = encoded.AsSpan();
            var destinationSpan = encoded.Length <= MaxSpanLength
                ? stackalloc char[encoded.Length]
                : new char[encoded.Length];

            sourceSpan.Replace(destinationSpan, '_', '/');
            destinationSpan.Replace('-', '+');

            encoded = destinationSpan.ToString();
        }

        var bytes = encoded.Length >= MaxSpanLength
            ? stackalloc byte[encoded.Length]
            : new byte[encoded.Length];

        var valid = Convert.TryFromBase64String(encoded, bytes, out var length);

        text = valid ? Encoding.UTF8.GetString(bytes[..length]) : null;

        return valid;
    }
}