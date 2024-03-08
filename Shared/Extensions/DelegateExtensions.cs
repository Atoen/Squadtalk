namespace Shared.Extensions;

public static class DelegateExtensions
{
    public static Task TryInvoke<T1, T2>(this Func<T1, T2, Task>? @delegate, T1 arg1, T2 arg2)
    {
        if (@delegate is not null)
        {
            return @delegate(arg1, arg2);
        }

        return Task.CompletedTask;
    }
    
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