namespace Shared.Data;

public class WeakRefCollection<T> where T : class
{
    private readonly List<WeakReference<T>> _weakReferences = [];

    public void Add(T element)
    {
        lock (_weakReferences)
        {
            RemoveDeadReferences();

            if (_weakReferences.Exists(x => x.TryGetTarget(out var target) && ReferenceEquals(target, element)))
            {
                return;
            }

            _weakReferences.Add(new WeakReference<T>(element));
        }
    }

    public void ForEach(Action<T> action)
    {
        lock (_weakReferences)
        {
            RemoveDeadReferences();

            foreach (var reference in _weakReferences)
            {
                if (reference.TryGetTarget(out var target))
                {
                    action(target);
                }
            }
        }
    }

    private void RemoveDeadReferences()
    {
        lock (_weakReferences)
        {
            for (var i = _weakReferences.Count - 1; i >= 0; i--)
            {
                if (!_weakReferences[i].TryGetTarget(out _))
                {
                    _weakReferences.RemoveAt(i);
                }
            }
        }
    }
}
