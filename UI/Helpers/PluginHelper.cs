using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Models.Data;
using Models.Helpers;
using Models.Interfaces;

namespace UI.Helpers;

public static class PluginHelper
{
    private static Dictionary<PluginManifest, IPlugin?> loadedPlugins = new();

    public static PluginManifest[] GetActivePlugins()
        => loadedPlugins.Keys.ToArray();

    public static IPlugin? GetPlugin(PluginManifest manifest)
        => loadedPlugins[manifest];

    public static async Task LoadPlugins()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Plugins");

        if (Directory.Exists(root))
        {
            foreach (string pluginFolder in Directory.GetDirectories(root))
            {
                string manifestLoc = Path.Combine(pluginFolder, "manifest.json");

                if (!File.Exists(manifestLoc))
                {
                    LoggingHelper.LogWarning($"Plugin folder contained no manifest.json");
                    continue;
                }

                IPlugin? plugin = null;
                PluginLoader? loader = null;
                PluginManifest? manifest = null;

                try
                {
                    string json = await File.ReadAllTextAsync(manifestLoc);
                    manifest = JsonSerializer.Deserialize<PluginManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    LoggingHelper.LogError($"Failed deserialize plugin - {pluginFolder}");
                    continue;
                }

                try
                {
                    string entrypoint = Path.Combine(pluginFolder, manifest.Value.Entrypoint);

                    if (!File.Exists(entrypoint))
                        throw new Exception($"[{manifest.Value.Name}] Plugins entrypoint not found");

                    loader = new PluginLoader(entrypoint);
                    Assembly assembly = loader.LoadFromAssemblyPath(Path.GetFullPath(entrypoint));

                    var entryPointType = assembly.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                    if (entryPointType == null)
                        throw new Exception($"[{manifest.Value.Name}] Plugin entrypoint doesn't correctly inherit from IPlugin");

                    plugin = (IPlugin)Activator.CreateInstance(entryPointType)!;
                    await plugin.Register();
                }
                catch (Exception e)
                {
                    plugin = null;
                    LoggingHelper.LogError($"PLUGIN {manifest.Value.Name} - {e.Message}");

                    try
                    {
                        loader?.Unload();
                    }
                    catch
                    {
                        LoggingHelper.LogWarning($"PLUGIN {manifest.Value.Name} - Failed to properly unload plugin");
                    }
                }

                loadedPlugins.Add(manifest.Value, plugin);
            }
        }
    }

    public static LoadRequest[] GetPluginLoads()
        => loadedPlugins.Where(l => l.Value != null).SelectMany(l => l.Value!.OnLaunch()).ToArray();
}
