using System.Diagnostics;
using JetBrains.Annotations;

namespace Shared.Reactive;

public abstract class Observable<T> : IObservable where T : class
{
    private List<ISubscriber>? _subscribers;
    private readonly Lock _lock = new();
    
    public static explicit operator T(Observable<T> observable)
    {
        var value = observable as T;
        Debug.Assert(value is not null);

        return value;
    }

    [MustUseReturnValue]
    public virtual IDisposable? Subscribe(ISubscriber subscriber)
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
        Notify();
    }

    protected void Notify()
    {
        if (_subscribers is not { Count: > 0 } subscribers)
        {
            return;
        }

        lock (_lock)
        {
            foreach (var subscriber in subscribers)
            {
                subscriber.OnChange();
            }
        }
    }

    private class Unsubscriber(List<ISubscriber> subscribers, ISubscriber subscriber, Lock @lock) : IDisposable
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
