using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Embedding.Offscreen;
using Avalonia.Media;
using Logic;
using Models;
using Models.Interfaces;

namespace UI.Helpers;

public static class ThemeHelper
{
    public static string currentThemeName { get; private set; } = "";

    private static string[] GetThemeRoots() => [Path.Combine(AppContext.BaseDirectory, "Themes"), Path.Combine(GlobalConfig.getDataFolder, "Themes")];

    public static async Task Startup()
    {
        string theme = await DependencyManager.GetService<IConfigLogic>()!.Get(Models.Enums.ConfigEntry.ActiveTheme, "default");
        await ChangeTheme(theme);
    }

    public static async Task<bool> ChangeTheme(string? to)
    {
        if (string.IsNullOrEmpty(to))
            to = "default";

        string? themePath = null;
        foreach (string root in GetThemeRoots())
        {
            if (!Directory.Exists(root))
                continue;

            string path = Path.Combine(root, $"{to}.json");

            if (File.Exists(path))
            {
                themePath = path;
                break;
            }
        }

        if (string.IsNullOrEmpty(themePath))
            return false;

        using (StreamReader reader = new StreamReader(themePath))
        {
            string json = await reader.ReadToEndAsync();
            JsonDocument doc = JsonDocument.Parse(json);

            foreach (JsonProperty el in doc.RootElement.EnumerateObject())
            {
                try
                {
                    string key = el.Name;
                    string? colour = el.Value.GetString();

                    if (string.IsNullOrEmpty(colour))
                        continue;

                    Application.Current!.Resources[$"Colour_{key}"] = Color.Parse(colour);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to apply theme part - {el.Name}\n{e.Message}");
                }
            }
        }

        currentThemeName = to;
        await DependencyManager.GetService<IConfigLogic>()!.Set(Models.Enums.ConfigEntry.ActiveTheme, currentThemeName, true);

        return true;
    }

    public static string[] GetThemes()
    {
        return ["default", .. GetThemeRoots().SelectMany(SearchDir)];

        string[] SearchDir(string root)
        {
            if (!Directory.Exists(root))
                return [];

            return Directory.GetFiles(root)
                .Where(f => !f.EndsWith("default.json") && f.EndsWith(".json"))
                .Select(f => Path.GetFileName(f).Replace(".json", ""))
                .ToArray();
        }
    }
}
