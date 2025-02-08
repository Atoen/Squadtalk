using JetBrains.Annotations;

namespace Shared.Reactive;

public interface IObservable
{
    [MustUseReturnValue]
    IDisposable? Subscribe(ISubscriber subscriber);
}