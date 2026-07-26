using System.Text.Json;
using Models.Enums;
using Models.Interfaces;

namespace Logic;

public class ConfigLogic : IConfigLogic
{
    public IDataRepository data => DependencyManager.GetService<IDataRepository>()!;
    private Dictionary<ConfigEntry, string?[]> cache = new();

    public async Task<T> Get<T>(ConfigEntry key, T defaultVal)
    {
        string?[] values;

        if (!cache.TryGetValue(key, out values!))
        {
            values = await data.GetConfigValue(key.ToString());
            cache[key] = values;
        }

        string? firstValue = values.FirstOrDefault();

        if (string.IsNullOrEmpty(firstValue))
            return defaultVal;

        if (typeof(T).IsEnum)
        {
            string? enumName = JsonSerializer.Deserialize<string>(firstValue);

            if (string.IsNullOrEmpty(enumName))
                return defaultVal;

            if (Enum.TryParse(typeof(T), enumName, out object? res) && res != null)
                return (T)res;

            return defaultVal;
        }

        return JsonSerializer.Deserialize<T>(firstValue) ?? defaultVal;
    }

    public async Task Set<T>(ConfigEntry key, T? value, bool removeIfEmpty)
    {
        if (value == null && removeIfEmpty)
        {
            await data.DeleteConfigValue(key.ToString());
            return;
        }

        string json = JsonSerializer.Serialize(value);
        await data.SetConfigValue(key.ToString(), json);

        cache.Remove(key);
    }
}
