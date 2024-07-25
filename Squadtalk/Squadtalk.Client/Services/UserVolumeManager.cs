using Blazored.LocalStorage;
using Shared.Data.TypedIds;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class UserVolumeManager(ILocalStorageService localStorageService)
{
    private readonly Func<UserId, string> _keyGenerator = id => $"v_{id.ToString()}";

    public async ValueTask SaveUserVolumeAsync(UserId userId, Volume volume)
    {
        var key = _keyGenerator(userId);
        await localStorageService.SetItemAsync(key, volume);
    }

    public async ValueTask<Volume> GetUserVolumeAsync(UserId userId)
    {
        var key = _keyGenerator(userId);
        var containsKey = await localStorageService.ContainKeyAsync(key);

        var volume = containsKey ?
            await localStorageService.GetItemAsync<Volume>(key)
            : Volume.Full;

        return volume;
    }
}
