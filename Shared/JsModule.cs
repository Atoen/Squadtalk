namespace Shared;

using static JsModuleLocation;

public record JsModule(string Name, JsModuleLocation Location = Unspecified)
{
    public static readonly JsModule WebRTC = new("WebRTC", ScriptsFolder);

    public static readonly JsModule MediaQuery = new("MediaQuery", ScriptsFolderMinified);

    public static readonly JsModule CanvasFill = new("CanvasFill", ScriptsFolderMinified);

    public static readonly JsModule FileTransfer = new("FileTransfer", ScriptsFolderMinified);

    public static readonly JsModule Simulation = new("Simulation", ScriptsFolderMinified);

    public static readonly JsModule VoiceChat = new("VoiceChat", ScriptsFolderMinified);

    public static readonly JsModule InfiniteScrolling = new("InfiniteScrolling", Collocated);

    public static readonly JsModule InputBox = new("InputBox", Collocated);

    public static IEnumerable<string> GetPathsToTry(JsModule jsModule)
    {
        switch (jsModule.Location)
        {
            case Collocated:
                yield return $"../Components/{jsModule.Name}.razor.js";
                break;

            case ScriptsFolder:
                yield return $"../js/{jsModule.Name}.js";
                break;

            case ScriptsFolderMinified:
                yield return $"../js/{jsModule.Name}.min.js";
                break;

            case Unspecified:
            default:
                yield return $"../Components/{jsModule.Name}.razor.js";
                yield return $"../js/{jsModule.Name}.min.js";
                yield return $"../js/{jsModule.Name}.js";
                break;
        }
    }
}

public enum JsModuleLocation
{
    Unspecified,
    Collocated,
    ScriptsFolder,
    ScriptsFolderMinified
}
