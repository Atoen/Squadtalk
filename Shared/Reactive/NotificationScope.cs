namespace Shared.Reactive;

public struct NotificationScope : IDisposable
{
    private readonly IUseNotificationScope _scoped;
    
    public bool HasPendingNotifications { get; private set; }

    public NotificationScope(IUseNotificationScope scoped)
    {
        _scoped = scoped;
        _scoped.EnterScope(this);
    }

    public void MarkChanges() => HasPendingNotifications = true;

    public void Dispose() => _scoped.ExitScope(this);
}

public interface IUseNotificationScope
{
    void EnterScope(NotificationScope scope);
    
    void ExitScope(NotificationScope scope);
}