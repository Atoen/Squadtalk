namespace Shared.Reactive;

public interface ISubscriber<in T>
{
    void OnNext(T value);
}

public interface IObservable<out T>
{
    IDisposable? Subscribe(ISubscriber<T> subscriber);
}