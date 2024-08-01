namespace Shared.Services;

public interface IPersistState
{
    const string Channels = nameof(Channels);
    const string Users = nameof(Users);

    Task PersistDataAsync();
}
