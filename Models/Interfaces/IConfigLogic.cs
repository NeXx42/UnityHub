using Models.Enums;

namespace Models.Interfaces;

public interface IConfigLogic
{
    public Task<T> Get<T>(ConfigEntry key, T defaultVal);
    public Task Set<T>(ConfigEntry key, T? value, bool removeIfNull);
}
