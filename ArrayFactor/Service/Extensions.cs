using System;

namespace ArrayFactor.Service;

internal static class Extensions
{
    public static T? GetService<T>(this IServiceProvider provider)
        where T : class => provider.GetService(typeof(T)) as T;

    public static void UseService<T>(this IServiceProvider provider, Action<T> action)
        where T : class
    {
        if(provider.GetService(typeof(T)) is T t) action(t);
    }

    public static Q UseService<T, Q>(this IServiceProvider provider, Func<T, Q> action, Q Default = default)
        where T : class =>
        provider.GetService(typeof(T)) is T t ? action(t) : Default;
}