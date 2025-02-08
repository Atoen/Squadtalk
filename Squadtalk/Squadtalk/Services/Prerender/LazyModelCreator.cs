using Shared.Reactive;

namespace Squadtalk.Services.Prerender;

internal abstract class LazyModelCreator(PrerenderPersistantState prerenderPersistantState)
{
    private bool _modelsCreated;

    protected ObservableDictionary<TKey, TValue> TryCreateModels<TKey, TValue>(
        ref readonly ObservableDictionary<TKey, TValue>? storage, ObservableDictionary<TKey, TValue> empty)
        where TKey : notnull where TValue : class, IKeyId<TKey>
    {
        if (!prerenderPersistantState.ContainsData)
        {
            return _modelsCreated ? storage ?? empty : empty;
        }

        if (!_modelsCreated)
        {
            CreateModels(prerenderPersistantState);
            _modelsCreated = true;
        }

        return storage ?? empty;
    }

    protected abstract void CreateModels(PrerenderPersistantState prerenderPersistantState);
}
