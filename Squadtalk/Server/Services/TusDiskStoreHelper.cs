using tusdotnet.Stores;

namespace Squadtalk.Server.Services;

public class TusDiskStoreHelper
{
    public TusDiskStoreHelper(IConfiguration configuration)
    {
        var path = configuration["Tus:Address"];
        ArgumentException.ThrowIfNullOrEmpty(path);

        Path = path;
    }

    public string Path { get; }

    public TusDiskStore Store => new(Path);
}