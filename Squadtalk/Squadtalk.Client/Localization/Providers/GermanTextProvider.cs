namespace Squadtalk.Client.Localization.Providers;

public class GermanTextProvider() : LocalizedTextProvider(Dictionary)
{
    private static Dictionary<string, string> Dictionary => new()
    {
        [nameof(TextTable.HelloWorld)] = "HHellen machen!"
    };
}
