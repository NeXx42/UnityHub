using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models.Helpers;
using UI.Interfaces;

namespace UI.Helpers;

public class FrontendPluginHandler<T> where T : IFrontendPlugin
{
    private List<T> plugins = new List<T>();

    public NEWTYPE Register<NEWTYPE>() where NEWTYPE : T
    {
        NEWTYPE obj = Activator.CreateInstance<NEWTYPE>();
        plugins.Add(obj);

        return obj;
    }

    public NEWTYPE Register<NEWTYPE>(NEWTYPE plugin) where NEWTYPE : T
    {
        plugins.Add(plugin);
        return plugin;
    }

    public async Task Execute(Func<T, Task> callback)
    {
        foreach (T plugin in plugins)
        {
            try
            {
                await callback(plugin);
            }
            catch (NotImplementedException) { }
            catch
            {
                LoggingHelper.LogError("Plugin failed to exec callback");
            }
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
            catch (NotImplementedException) { }
            catch
            {
                LoggingHelper.LogError("Plugin failed to exec callback");
            }
        }
    }
}
