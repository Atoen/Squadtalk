using System.ComponentModel;
using System.Globalization;

namespace Shared.Data.TypedIds.TypeConverters;

public class StringIdConverter<T> : TypeConverter where T : IIdRecord<T, string>
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
        if (value is string val)
        {
            return T.Create(val);
        }
            
        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is T id)
        {
            return id.Value;
        }
            
        return base.ConvertTo(context, culture, value, destinationType);
    }
}

public class GuidIdConverter<T> : TypeConverter where T : IIdRecord<T, Guid>
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
            Guid guid => T.Create(guid),
            string str when Guid.TryParse(str, out var guid) => T.Create(guid),
            _ => base.ConvertFrom(context, culture, value)
        };
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is T id)
        {
            if (destinationType == typeof(Guid)) return id.Value;
            if (destinationType == typeof(string)) return id.Value.ToString();
        }
            
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
