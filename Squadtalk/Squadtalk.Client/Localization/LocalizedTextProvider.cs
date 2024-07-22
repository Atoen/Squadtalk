namespace Squadtalk.Client.Localization;

public abstract class LocalizedTextProvider(Dictionary<string, string> dictionary)
{
    public string this[string key] => dictionary.GetValueOrDefault(key, key);
}
