namespace Shared.Extensions;

public static class DelegateExtensions
{
    public static Task TryInvoke<T>(this Func<T, Task>? @delegate, T arg)
    {
        if (@delegate is not null)
        {
            return @delegate(arg);
        }

        return Task.CompletedTask;
    }
    
    public static Task TryInvoke(this Func<Task>? @delegate)
    {
        if (@delegate is not null)
        {
            return @delegate();
        }

        return Task.CompletedTask;
    }
}