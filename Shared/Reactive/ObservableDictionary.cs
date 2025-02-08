using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Reactive;

public class ObservableDictionary<TKey, TValue>
    : Observable<ObservableDictionary<TKey, TValue>>, 
      IObservableCollection<KeyValuePair<TKey, TValue>>
    where TKey : notnull
    where TValue : class, IKeyId<TKey>
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private ValueCollection? _values;
    
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }
    
    public int Count => _dictionary.Count;
    
    public ObservableDictionary() => _dictionary = [];

    public ObservableDictionary(Dictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

    public IObservableCollection<TValue> Values => _values ??= new ValueCollection(this);

    public bool Add(TValue value)
    {
        var added = _dictionary.TryAdd(value.Key, value);
        if (added)
        {
            Notify();
        }

        return added;
    }
    
    public void AddMany(params IEnumerable<TValue> values)
    {
        foreach (var value in values)
        {
            _dictionary.Add(value.Key, value);
        }
        
        Notify();
    }
    
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => 
        _dictionary.TryGetValue(key, out value);

    public TValue? GetValueOrDefault(TKey key) => _dictionary.GetValueOrDefault(key);

    public TValue GetValueOrDefault(TKey key, TValue defaultValue) => _dictionary.GetValueOrDefault(key, defaultValue);

    public bool Remove(TKey key)
    {
        var removed = _dictionary.Remove(key);
        if (removed)
        {
            Notify();
        }

        return removed;
    }
    
    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var removed = _dictionary.Remove(key, out value);
        if (removed)
        {
            Notify();
        }
        
        return removed;
    }
    
    public bool Remove(TValue value)
    {
        var removed = _dictionary.Remove(value.Key);
        if (removed)
        {
            Notify();
        }

        return removed;
    }

    public void Clear()
    {
        if (Count == 0) return;
        
        _dictionary.Clear();
        Notify();
    }

    public void Refresh(params IEnumerable<TValue> values)
    {
        var startCount = Count;
        _dictionary.Clear();
        foreach (var value in values)
        {
            _dictionary.Add(value.Key, value);
        }

        var refreshedCount = Count;
        if (startCount != refreshedCount || refreshedCount != 0)
        {
            Notify();
        }
    }

    private sealed class ValueCollection(ObservableDictionary<TKey, TValue> observableDictionary) : IObservableCollection<TValue>
    {
        public IDisposable? Subscribe(ISubscriber subscriber) => observableDictionary.Subscribe(subscriber);
        
        public IEnumerator<TValue> GetEnumerator() => observableDictionary._dictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => observableDictionary._dictionary.Values.Count;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}