using Shared.Data.TypedIds;

namespace Shared.Models;

public class TypingUsers
{
    private readonly Dictionary<UserId, TypingUser> _typingUsers = [];
    private static readonly TimeSpan StaleTimeout = TypingTiming.StaleRemoveTimeout;

    public int Count => _typingUsers.Count;
    public IReadOnlyCollection<TypingUser> Typing => _typingUsers.Values;

    public bool InsertOrUpdate(UserModel user)
    {
        if (_typingUsers.TryGetValue(user.Id, out var typingUser))
        {
            typingUser.LastUpdate = DateTime.Now;
            return false;
        }

        return _typingUsers.TryAdd(user.Id, new TypingUser(user));
    }

    public bool Remove(UserId userId)
    {
        return _typingUsers.Remove(userId);
    }

    public bool RemoveStale(DateTime now)
    {
        if (_typingUsers.Count == 0)
        {
            return false;
        }

        var removedAny = false;

        foreach (var (userId, typingUser) in _typingUsers.ToList())
        {
            if (now - typingUser.LastUpdate > StaleTimeout)
            {
                _typingUsers.Remove(userId);
                removedAny = true;
            }
        }

        return removedAny;
    }
}

public class TypingUser(UserModel user)
{
    public UserModel User { get; } = user;

    public DateTime LastUpdate { get; set; } = DateTime.Now;

    public string Username => User.Username;
}

public static class TypingTiming
{
    public static readonly TimeSpan StartDelayToNotify = TimeSpan.FromSeconds(1);

    public static readonly TimeSpan InputBoxInterval = TimeSpan.FromSeconds(5);

    public static readonly TimeSpan StaleScanInterval = TimeSpan.FromSeconds(10);

    public static readonly TimeSpan StaleRemoveTimeout = TimeSpan.FromSeconds(10);
}
