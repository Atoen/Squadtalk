using System.Diagnostics;
using JetBrains.Annotations;

namespace Shared.Reactive;

public abstract class Observable<T> : IObservable, IUseNotificationScope where T : class
{
    private List<ISubscriber>? _subscribers;
    private readonly Lock _lock = new();

    private NotificationScope? _activeScope;

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
                Console.WriteLine($"[{typeof(Observable<T>)}] Subscribed! {_subscribers.Count}");
            }
            else
            {
                Console.WriteLine($"[{typeof(Observable<T>)}] Failed to subscribe!, {_subscribers.Count}");
            }
        }

        return new Unsubscriber(this, subscriber);
    }

    protected void SetField<TField>(ref TField field, TField value)
    {
        if (EqualityComparer<TField>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        Notify();
    }

    protected void Notify(bool force = false)
    {
        if (_activeScope is { } scope && !force)
        {
            scope.MarkChanges();
            return;
        }

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

    private void ClearSubscriber(ISubscriber subscriber)
    {
        if (_subscribers is not { Count: > 0 } subscribers)
        {
            Console.WriteLine($"[{typeof(Observable<T>)}] Failed to unsubscribe!");
            return;
        }

        lock (_lock)
        {
            if (subscribers.Remove(subscriber))
            {
                Console.WriteLine($"[{typeof(Observable<T>)}] Unsubscribed!, {subscribers.Count}");
            }
            else
            {
                Console.WriteLine($"[{typeof(Observable<T>)}] Failed to unsubscribe!, {subscribers.Count}");
            }
        }
    }

    private sealed class Unsubscriber(Observable<T> observable, ISubscriber subscriber) : IDisposable
    {
        public void Dispose() => observable.ClearSubscriber(subscriber);
    }

    public void EnterScope(in NotificationScope scope)
    {
        _activeScope = scope;
    }

    public void ExitScope(in NotificationScope scope)
    {
        if (scope.HasPendingNotifications)
        {
            Notify(force: true);
        }

        _activeScope = null;
    }
}
