namespace Shared.Data.TypedIds;

public abstract record StringIdRecord
{
    protected StringIdRecord(string value)
    {
        Value = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Value must be non-empty", nameof(value));
    }
    
    public string Value { get; }
    
    public static implicit operator string(StringIdRecord id) => id.Value;
}
