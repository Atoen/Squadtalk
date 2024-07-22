using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shared.Extensions;

public static class StringExtensions
{
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
        Span<char> destinationSpan = stackalloc char[encoded.Length];
 
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
            Span<char> destinationSpan = stackalloc char[encoded.Length];
            
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

        Span<byte> bytes = stackalloc byte[encoded.Length];
        
        if (urlEncoded)
        {
            var sourceSpan = encoded.AsSpan();
            Span<char> destinationSpan = stackalloc char[encoded.Length];
            
            sourceSpan.Replace(destinationSpan, '_', '/');
            destinationSpan.Replace('-', '+');

            encoded = destinationSpan.ToString();
        }
        
        var valid = Convert.TryFromBase64String(encoded, bytes, out var length);

        text = valid ? Encoding.UTF8.GetString(bytes[..length]) : null;

        return valid;
    }

    public static string TryFormat(this string template, object? arg0)
    {
        try
        {
            return string.Format(template, arg0);
        }
        catch
        {
            return template;
        }
    }


    public static string TryFormat(this string template, object? arg0, object? arg1)
    {
        try
        {
            return string.Format(template, arg0, arg1);
        }
        catch
        {
            return template;
        }
    }

    public static string TryFormat(this string template, object? arg0, object? arg1, object? arg2)
    {
        try
        {
            return string.Format(template, arg0, arg1, arg2);
        }
        catch
        {
            return template;
        }
    }

    public static string TryFormat(this string template, params object?[] args)
    {
        try
        {
            return string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }
}