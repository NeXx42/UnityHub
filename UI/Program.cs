using System;
using System.Threading.Tasks;
using Avalonia;
using Data.DataRepos;
using Data_Sqlite;
using Logic;

namespace UI;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        _ = Setup();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        async Task Setup()
        {
            await DependencyManager.RegisterDataRepo<SqliteDataRepo>();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
