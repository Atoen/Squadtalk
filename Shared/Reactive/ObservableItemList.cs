using System.Collections;

namespace Shared.Reactive;

public sealed class ObservableItemList<T>
    : Observable<ObservableItemList<T>>,
        IObservableCollection<T>,
        ISubscriber,
        IDisposable
    where T : IObservable
{
    private readonly List<T> _list;
    private readonly List<IDisposable?> _subscriptions;

    public int Count => _list.Count;

    public ObservableItemList()
    {
        _list = [];
        _subscriptions = [];
    }

    public ObservableItemList(List<T> list)
    {
        _list = list;
        _subscriptions = list.Select(x => x.Subscribe(this)).ToList();
    }

    public void Add(T value)
    {
        _list.Add(value);
        _subscriptions.Add(value.Subscribe(this));

        Notify();
    }

    public void AddMany(params IEnumerable<T> values)
    {
        var array = values as T[] ?? values.ToArray();
        _list.AddRange(array);
        _subscriptions.AddRange(array.Select(x => x.Subscribe(this)));

        Notify();
    }

    public bool Remove(T value)
    {
        var index = _list.IndexOf(value);
        if (index == -1)
        {
            return false;
        }

        var removed = _list.Remove(value);
        if (removed)
        {
            var subscription = _subscriptions[index];
            subscription?.Dispose();

            _subscriptions.RemoveAt(index);

            Notify();
        }

        return removed;
    }

    public void Clear()
    {
        if (Count == 0) return;

        _list.Clear();
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }

        Notify();
    }

    public void Refresh(IEnumerable<T> values)
    {
        var startCount = Count;

        _list.Clear();
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }

        var array = values as T[] ?? values.ToArray();
        _list.AddRange(array);
        _subscriptions.AddRange(array.Select(x => x.Subscribe(this)));


        var refreshedCount = Count;

        if (startCount != refreshedCount || refreshedCount != 0)
        {
            Notify();
        }
    }

    public void OnChange() => Notify();

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
    }
}