namespace Squadtalk.Services.Prerender;

internal abstract class LazyModelCreator(PrerenderPersistantState prerenderPersistantState)
{
    private bool _modelsCreated;

    protected Dictionary<TKey, TValue> TryCreateModels<TKey, TValue>(
        ref readonly Dictionary<TKey, TValue>? storage, Dictionary<TKey, TValue> empty)
        where TKey : notnull
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
