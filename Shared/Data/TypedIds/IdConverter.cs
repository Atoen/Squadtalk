using System.ComponentModel;
using System.Globalization;

namespace Shared.Data.TypedIds;

public class StringIdConverter<T> : TypeConverter where T : class, IStringIdRecord<T>
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value is string val
            ? T.From(val)
            : base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return destinationType == typeof(string) && value is T id
            ? id.Value
            : base.ConvertTo(context, culture, value, destinationType);
    }
}

public class GuidIdConverter<T> : TypeConverter where T : struct, IGuidIdRecord<T>
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(Guid) || sourceType == typeof(string)
                                          || base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(Guid) || destinationType == typeof(string)
                                               || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            Guid guid => T.From(guid),
            string str when T.TryParse(str, out var idRecord) => idRecord,
            _ => base.ConvertFrom(context, culture, value)
        };
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return value switch
        {
            T id when destinationType == typeof(Guid) => id.Value,
            T id when destinationType == typeof(string) => id.Value.ToString(),
            _ => base.ConvertTo(context, culture, value, destinationType)
        };
    }
}
