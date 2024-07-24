namespace Squadtalk.Client.Localization;

public abstract class LocalizedTextProvider(Dictionary<string, string> dictionary)
{
    public string this[string key] => dictionary.GetValueOrDefault(key, key);
}

public readonly record struct StringTemplate(string Template)
{
    public static implicit operator StringTemplate(string template) => new(template);

    public string TryFormat(object? arg0)
    {
        try
        {
            return string.Format(Template, arg0);
        }
        catch
        {
            return Template;
        }
    }

    public string TryFormat(object? arg0, object? arg1)
    {
        try
        {
            return string.Format(Template, arg0, arg1);
        }
        catch
        {
            return Template;
        }
    }

    public string TryFormat(object? arg0, object? arg1, object? arg2)
    {
        try
        {
            return string.Format(Template, arg0, arg1, arg2);
        }
        catch
        {
            return Template;
        }
    }

    public string TryFormat(params object?[] args)
    {
        try
        {
            return string.Format(Template, args);
        }
        catch
        {
            return Template;
        }
    }
}
