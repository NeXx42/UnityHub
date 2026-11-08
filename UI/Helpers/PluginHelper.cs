using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Logic;
using Models;
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
        IConfigLogic config = DependencyManager.GetService<IConfigLogic>()!;
        Dictionary<Guid, string> cachedPlugins = await config.Get<Dictionary<Guid, string>>(Models.Enums.ConfigEntry.PluginCache, []);

        string root = Path.Combine(AppContext.BaseDirectory, "Plugins");

        if (Directory.Exists(root))
        {
            string[] acceptablePlugins = Directory.GetFiles(root, "*.plugin");

            foreach (string pluginArchive in acceptablePlugins)
            {
                string pluginFolder = await ExtractPlugin(pluginArchive);
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

                if (cachedPlugins.TryGetValue(manifest.Value.Id, out string? oldPath) && Directory.Exists(oldPath) && !pluginFolder.Equals(oldPath))
                    Directory.Delete(oldPath, true);

                cachedPlugins[manifest.Value.Id] = pluginFolder;

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

        await config.Set(Models.Enums.ConfigEntry.PluginCache, cachedPlugins, true);
    }

    private static async Task<string> ExtractPlugin(string archivePath)
    {
        string cacheFolder = Path.Combine(GlobalConfig.getDataFolder, "plugins");
        Directory.CreateDirectory(cacheFolder);

        using var stream = File.OpenRead(archivePath);
        byte[] hashBytes = SHA256.HashData(stream);
        string hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        string pluginCacheFolder = Path.Combine(cacheFolder, hash);

        if (Directory.Exists(pluginCacheFolder))
            return pluginCacheFolder;

        ZipFile.ExtractToDirectory(archivePath, pluginCacheFolder, overwriteFiles: true);
        return pluginCacheFolder;
    }

    public static LoadRequest[] GetPluginLoads()
        => loadedPlugins.Where(l => l.Value != null).SelectMany(l => l.Value!.OnLaunch()).ToArray();
}
