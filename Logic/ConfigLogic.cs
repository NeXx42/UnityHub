using System.Text.Json;
using Models.Enums;
using Models.Interfaces;

namespace Logic;

public class ConfigLogic : IConfigLogic
{
    public IDataRepository data => DependencyManager.GetService<IDataRepository>()!;

    public async Task<T> Get<T>(ConfigEntry key, T defaultVal)
    {
        string?[] values = await data.GetConfigValue(key.ToString());
        string? firstValue = values.FirstOrDefault();

        if (string.IsNullOrEmpty(firstValue))
            return defaultVal;

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
    }
}
