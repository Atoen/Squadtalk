namespace Squadtalk.Server.Services;

public class TusDiskStoreHelper
{
    public string Path { get; }

    public TusDiskStoreHelper(IConfiguration configuration)
    {
        var path = configuration["Tus:Address"];
        ArgumentException.ThrowIfNullOrEmpty(path);

        Path = path;
    }
}