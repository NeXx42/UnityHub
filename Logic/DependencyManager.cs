using Models.Interfaces;

namespace Logic;

public static class DependencyManager
{
    public static IDataRepository? dataRepo { private set; get; }

    public static async Task RegisterDataRepo<T>() where T : IDataRepository
    {
        dataRepo = Activator.CreateInstance<T>();
        await dataRepo.Setup();
    }
}
