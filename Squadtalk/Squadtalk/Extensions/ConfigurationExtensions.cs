using System.Diagnostics.CodeAnalysis;

namespace Squadtalk.Extensions;

public static class ConfigurationExtensions
{
    public static string GetString(this IConfiguration configuration, string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var result = configuration[key];
        if (result is null)
        {
            Throw(key);
        }

        return result;
    }

    [DoesNotReturn]
    private static void Throw(string key)
    {
        var factory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = factory.CreateLogger(typeof(ConfigurationExtensions));
        logger.LogCritical("Key {Key} is missing from configuration file", key);
        
        throw new ArgumentNullException(nameof(key), $"Key {key} missing from configuration file");
    }
}