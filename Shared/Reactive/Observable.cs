using System.Diagnostics;
using JetBrains.Annotations;

namespace Shared.Reactive;

public abstract class Observable<T> : IObservable<T> where T : class, IObservable<T>
{
    private List<ISubscriber<T>>? _subscribers;
    private readonly Lock _lock = new();
    
    public static explicit operator T(Observable<T> observable)
    {
        var value = observable as T;
        Debug.Assert(value is not null);

        return value;
    }

    [MustUseReturnValue]
    public IDisposable? Subscribe(ISubscriber<T> subscriber)
    {
        if (!OperatingSystem.IsBrowser()) return null;

        ArgumentNullException.ThrowIfNull(subscriber);

        lock (_lock)
        {
            _subscribers ??= [];

            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
                Console.WriteLine($"[{typeof(Observable<T>)}] Subscribed!");
            }
            else
            {
                Console.WriteLine($"[{typeof(Observable<T>)}] Failed to subscribe!");
            }
        }

        return new Unsubscriber(_subscribers, subscriber, _lock);
    }

    private protected void SetField<TField>(ref TField field, TField value)
    {
        if (EqualityComparer<TField>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        Notify((T) this);
    }

    protected void Notify(T value)
    {
        if (_subscribers is not { Count: > 0 } subscribers)
        {
            return;
        }

        lock (_lock)
        {
            foreach (var subscriber in subscribers)
            {
                subscriber.OnNext(value);
            }
        }
    }

    private class Unsubscriber(List<ISubscriber<T>> subscribers, ISubscriber<T> subscriber, Lock @lock) : IDisposable
    {
        public void Dispose()
        {
            lock (@lock)
            {
                if (subscribers.Remove(subscriber))
                {
                    Console.WriteLine($"[{typeof(Observable<T>)}] Unsubscribed!");
                }
                else
                {
                    Console.WriteLine($"[{typeof(Observable<T>)}] Failed to unsubscribe!");
                }
            }
        }
    }
}
