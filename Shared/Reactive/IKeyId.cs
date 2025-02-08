namespace Shared.Reactive;

public interface IKeyId<out T>
{
    T Key { get; }
}