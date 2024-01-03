using System.Text;
using tusdotnet.Models;

namespace Squadtalk.Extensions;

public static class MetadataExtensions
{
    public static string GetString(this Dictionary<string, Metadata> metadata, string key)
    {
        return metadata[key].GetString(Encoding.UTF8);
    }
}