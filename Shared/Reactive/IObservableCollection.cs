using System.Collections;

namespace Shared.Reactive;

public interface IObservableCollection<out T> : IObservable, IReadOnlyCollection<T>;

public class ObservableCollection<T> : IObservableCollection<T>
{
    public static readonly IObservableCollection<T> Empty = new ObservableCollection<T>();

    public IDisposable? Subscribe(ISubscriber subscriber) => null;

    public IEnumerator<T> GetEnumerator() => Enumerable.Empty<T>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => 0;
}