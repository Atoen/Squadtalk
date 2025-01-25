using StackExchange.Redis;

namespace Squadtalk.Redis;

public static partial class RedisExtensions
{
    public static WebApplication SetupRedisData(this WebApplication application)
    {
        var logger = application.Services.GetRequiredService<ILogger<Program>>();
        LogStart(logger);

        try
        {
            var mux = application.Services.GetRequiredService<IConnectionMultiplexer>();
            var db = mux.GetDatabase(2);

            var typingScript = File.ReadAllText("./Scripts/lua/typing.lua");
            db.Execute("FUNCTION", "LOAD", "REPLACE", typingScript);

            var userConnectionScript = File.ReadAllText("./Scripts/lua/userConnections.lua");
            db.Execute("FUNCTION", "LOAD", "REPLACE", userConnectionScript);
            db.Execute("FCALL", "clear_connections", 0);

            LogSuccess(logger);
            return application;
        }
        catch (Exception e)
        {
            LogFailure(logger, e);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Starting Redis setup...")]
    private static partial void LogStart(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Successfully setup Redis")]
    private static partial void LogSuccess(ILogger logger);

    [LoggerMessage(LogLevel.Critical, "Failed to setup Redis")]
    private static partial void LogFailure(ILogger logger, Exception exception);
}
