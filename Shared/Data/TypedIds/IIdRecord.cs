namespace Shared.Data.TypedIds;

public interface IStringIdRecord<out T> : IIdRecord<T, string> where T : class, IStringIdRecord<T>
{
    static abstract T New(StringIdValueFormat format = StringIdValueFormat.GuidN);
}

public interface IGuidIdRecord<T> : IIdRecord<T, Guid> where T : struct, IIdRecord<T, Guid>
{
    static abstract T New { get; }

    static abstract T Parse(ReadOnlySpan<char> span);

    static abstract bool TryParse(ReadOnlySpan<char> span, out T idRecord);
}

public interface IIdRecord<out TSelf, TValue>
{
    TValue Value { get; }

    static abstract TSelf From(TValue value);
}
