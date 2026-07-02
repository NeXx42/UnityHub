using System.Collections.Concurrent;
using Models.Interfaces;

namespace Logic;

public static class DependencyManager
{
    private static readonly ConcurrentDictionary<Type, object> activeServices = new();

    public static void RegisterService<TServiceType, TServiceImplementation>()
        where TServiceType : class
        where TServiceImplementation : class, TServiceType
    {
        _ = RegisterService<TServiceType, TServiceImplementation>(() => Task.FromResult(Activator.CreateInstance<TServiceImplementation>()));
    }

    public static async Task RegisterService<TServiceType, TServiceImplementation>(Func<Task<TServiceImplementation>> factory)
        where TServiceType : class
        where TServiceImplementation : class, TServiceType
    {
        TServiceImplementation service = await factory();
        activeServices[typeof(TServiceType)] = service;
    }

    public static T? GetService<T>() where T : class
    {
        return activeServices.TryGetValue(typeof(T), out var service) ? (T)service : default;
    }
}
