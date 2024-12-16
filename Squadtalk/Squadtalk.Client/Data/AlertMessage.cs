using MudBlazor;

namespace Squadtalk.Client.Data;

public record AlertMessage(string Text, Severity Severity)
{
    public static AlertMessage Success(string text) => new(text, Severity.Success);

    public static AlertMessage Error(string text) => new(text, Severity.Error);

    public static AlertMessage Info(string text) => new(text, Severity.Info);

    public static AlertMessage Warning(string text) => new(text, Severity.Warning);
}
