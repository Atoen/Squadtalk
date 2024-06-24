namespace Shared.Data.TypedIds;

public interface IStringIdRecord<out T> : IIdRecord<T, string>;

public interface IGuidIdRecord<out T> : IIdRecord<T, Guid>;

public interface IIdRecord<out TId, TValue>
{
    TValue Value { get; }

    static abstract TId Create(TValue value);
}