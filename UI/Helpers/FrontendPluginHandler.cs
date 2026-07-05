using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Interfaces;

namespace UI.Helpers;

public class FrontendPluginHandler<T> where T : IFrontendPlugin
{
    private List<T> plugins = new List<T>();

    public void Register<NEWTYPE>(NEWTYPE plugin) where NEWTYPE : T
    {
        plugins.Add(plugin);
    }

    public async Task Execute(Func<T, Task> callback)
    {
        foreach (T plugin in plugins)
        {
            try
            {
                await callback(plugin);
            }
            catch { }
        }
    }

    public void Execute(Action<T> callback)
    {
        foreach (T plugin in plugins)
        {
            try
            {
                callback(plugin);
            }
            catch { }
        }
    }
}
