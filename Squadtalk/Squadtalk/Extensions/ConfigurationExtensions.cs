using System.Diagnostics.CodeAnalysis;

namespace Squadtalk.Extensions;

public static class ConfigurationExtensions
{
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(name);
        if (connectionString is null)
        {
            ThrowConnectionString(name);
        }

        return connectionString;
    }

    public static string GetString(this IConfiguration configuration, string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var result = configuration[key];
        if (result is null)
        {
            ThrowKey(key);
        }

        return result;
    }

    public static T GetRequired<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var result = configuration.Get<T>(null);
        if (result is null)
        {
            ThrowGet(typeof(T));
        }

        return result;
    }

    [DoesNotReturn]
    private static void ThrowConnectionString(string name)
    {
        var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole());
        var logger = factory.CreateLogger(typeof(ConfigurationExtensions));
        logger.LogCritical("Connection string {ConnectionString} not found", name);

        throw new ArgumentNullException(nameof(name), $"Connection string {name} not found");
    }

    [DoesNotReturn]
    private static void ThrowKey(string key)
    {
        var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole());
        var logger = factory.CreateLogger(typeof(ConfigurationExtensions));
        logger.LogCritical("Key {Key} is missing from configuration file", key);

        throw new ArgumentNullException(nameof(key), $"Key {key} missing from configuration file");
    }

    [DoesNotReturn]
    private static void ThrowGet(Type type)
    {
        var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole());
        var logger = factory.CreateLogger(typeof(ConfigurationExtensions));

        logger.LogCritical("Cannot retrieve {Type} from configuration file", type.Name);

        throw new ArgumentNullException(type.Name, $"Key {type.Name} missing from configuration file");
    }
}