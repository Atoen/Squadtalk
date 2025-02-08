using System.Collections;

namespace Shared.Reactive;

public class ObservableList<T> : Observable<ObservableList<T>>, IObservableCollection<T>
{
    private readonly List<T> _list;
    
    public int Count => _list.Count;

    public ObservableList() => _list = [];

    public ObservableList(List<T> list) => _list = list;
    
    public void Add(T value)
    {
        _list.Add(value);
        Notify();
    }
    
    public void AddMany(params IEnumerable<T> values)
    {
        _list.AddRange(values);
        Notify();
    }

    public bool Remove(T value)
    {
        var removed = _list.Remove(value);
        if (removed)
        {
            Notify();
        }

        return removed;
    }

    public void Clear()
    {
        if (Count == 0) return;
        
        _list.Clear();
        Notify();
    }


    public void Refresh(IEnumerable<T> values)
    {
        var startCount = Count;
        _list.Clear();
        _list.AddRange(values);
        
        var refreshedCount = Count;

        if (startCount != refreshedCount || refreshedCount != 0)
        {
            Notify();
        }
    }
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}