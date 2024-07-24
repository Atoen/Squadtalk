namespace Squadtalk.Client.Localization.Providers;

public class GermanTextProvider() : LocalizedTextProvider(Texts)
{
    private static Dictionary<string, string> Texts => new()
    {
        [nameof(TextTable.HelloWorld)] = "HHellen machen!"
    };
}
