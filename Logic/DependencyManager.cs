using Models.Interfaces;

namespace Logic;

public static class DependencyManager
{
    public static IDataRepository? dataRepo { private set; get; }

    public static async Task RegisterDataRepo<T>() where T : IDataRepository
    {
        dataRepo = Activator.CreateInstance<T>();
        await SetupDependency(dataRepo.Setup);
    }

    private static async Task SetupDependency(Func<Task> Setup)
    {
        try
        {
            await Setup();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to setup dependency\n{e.Message}");
        }
    }
}
