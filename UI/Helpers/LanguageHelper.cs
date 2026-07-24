using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Logic;
using Models.Interfaces;

namespace UI.Helpers;

public static class LanguageHelper
{
    public const string DEFAULT_LANGUAGE = "English";
    public static string currentLanguageName = DEFAULT_LANGUAGE;

    public static async Task Startup()
    {
        string lang = await DependencyManager.GetService<IConfigLogic>()!.Get(Models.Enums.ConfigEntry.ActiveLanguage, DEFAULT_LANGUAGE);
        await ChangeLanguage(lang);
    }

    public static async Task<bool> ChangeLanguage(string? to)
    {
        if (string.IsNullOrEmpty(to))
            to = DEFAULT_LANGUAGE;

        string? languagePath = Path.Combine(AppContext.BaseDirectory, "Languages", $"{to}.json");

        if (!File.Exists(languagePath))
            return false;

        using (StreamReader reader = new StreamReader(languagePath))
        {
            string json = await reader.ReadToEndAsync();
            JsonDocument doc = JsonDocument.Parse(json);

            foreach (JsonProperty el in doc.RootElement.EnumerateObject())
            {
                try
                {
                    string key = el.Name;
                    string? val = el.Value.GetString();

                    if (string.IsNullOrEmpty(val))
                        continue;

                    Application.Current!.Resources[$"LANG_{key}"] = val;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to apply language part - {el.Name}\n{e.Message}");
                }
            }
        }

        currentLanguageName = to;
        await DependencyManager.GetService<IConfigLogic>()!.Set(Models.Enums.ConfigEntry.ActiveLanguage, currentLanguageName, true);

        return true;
    }

    public static string[] GetLanguages()
    {
        return Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Languages")).Where(f => f.EndsWith(".json"))
                .Select(f => Path.GetFileName(f).Replace(".json", ""))
                .ToArray();
    }

    public static string? GetLanguageResource(string key, Dictionary<string, string>? replacements = null)
    {
        if (!(Application.Current?.TryGetResource($"LANG_{key}", null, out object? val) ?? false))
            return null;

        StringBuilder sb = new StringBuilder(val as string);

        if (replacements != null)
        {
            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                sb.Replace($"${replacement.Key}", replacement.Value);
            }
        }

        return sb.ToString();
    }
}
