namespace Shared.Reactive;

public static class ReactiveExtensions
{
    public static ObservableList<T> ToObservableList<T>(this IEnumerable<T> enumerable)
    {
        var list = enumerable as List<T> ?? enumerable.ToList();
        return new ObservableList<T>(list);
    }

    public static ObservableList<T> ToObservable<T>(this List<T> list)
    {
        return new ObservableList<T>(list);
    }

    public static ObservableItemList<T> ToObservableItemList<T>(this IEnumerable<T> enumerable) where T : IObservable
    {
        var list = enumerable as List<T> ?? enumerable.ToList();
        return new ObservableItemList<T>(list);
    }

    public static ObservableDictionary<TKey, TValue> ToObservableDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
        where TKey : notnull
        where TValue : class, IKeyId<TKey>
    {
        var dictionary = enumerable.ToDictionary(keySelector, valueSelector);
        return new ObservableDictionary<TKey, TValue>(dictionary);
    }

    public static ObservableDictionary<TKey, TValue> ToObservable<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
        where TValue : class, IKeyId<TKey>
    {
        return new ObservableDictionary<TKey, TValue>(dictionary);
    }
}