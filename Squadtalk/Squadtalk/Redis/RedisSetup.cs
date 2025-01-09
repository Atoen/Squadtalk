using StackExchange.Redis;

namespace Squadtalk.Redis;

public static class RedisExtensions
{
    public static WebApplication SetupRedisData(this WebApplication application)
    {
        var mux = application.Services.GetRequiredService<IConnectionMultiplexer>();
        var db = mux.GetDatabase(2);

        var script = File.ReadAllText("./Scripts/lua/userConnections.lua");
        db.Execute("FUNCTION", "LOAD", "REPLACE", script);
        db.Execute("DEL", "user:connection", "user:status");

        return application;
    }
}
