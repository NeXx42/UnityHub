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
        _ = RegisterService<TServiceType, TServiceImplementation>(_ => Task.CompletedTask);
    }

    public static async Task RegisterService<TServiceType, TServiceImplementation>(Func<TServiceImplementation, Task> factory)
        where TServiceType : class
        where TServiceImplementation : class, TServiceType
    {
        TServiceImplementation service = Activator.CreateInstance<TServiceImplementation>();
        await factory(service);

        activeServices[typeof(TServiceType)] = service;
    }

    public static T? GetService<T>() where T : class
    {
        return activeServices.TryGetValue(typeof(T), out var service) ? (T)service : default;
    }
}
