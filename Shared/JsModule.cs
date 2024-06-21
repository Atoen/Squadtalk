namespace Shared;

public record JsModule(string Name, JsModuleLocation Location = JsModuleLocation.Unspecified)
{
    public static readonly JsModule WebRTC = new("WebRTC", JsModuleLocation.ScriptsFolderMinified);

    public static readonly JsModule CanvasFill = new("CanvasFill", JsModuleLocation.ScriptsFolderMinified);

    public static readonly JsModule FileTransfer = new("FileTransfer", JsModuleLocation.ScriptsFolderMinified);

    public static readonly JsModule Simulation = new("Simulation", JsModuleLocation.ScriptsFolderMinified);

    public static readonly JsModule VoiceChat = new("VoiceChat", JsModuleLocation.ScriptsFolderMinified);

    public static readonly JsModule InfiniteScrolling = new("InfiniteScrolling", JsModuleLocation.Collocated);

    public static readonly JsModule InputBox = new("InputBox", JsModuleLocation.Collocated);
}

public enum JsModuleLocation
{
    Unspecified,
    Collocated,
    ScriptsFolder,
    ScriptsFolderMinified
}