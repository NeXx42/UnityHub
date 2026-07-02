using System;
using System.Threading.Tasks;
using Avalonia;
using Data.DataRepos;
using Data_Sqlite;
using Logic;
using Models.Interfaces;

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
            await DependencyManager.RegisterService<IDataRepository, SqliteDataRepo>(async () =>
            {
                SqliteDataRepo repo = new SqliteDataRepo();
                await repo.Setup();

                return repo;
            });

            DependencyManager.RegisterService<IEditorLogic, EditorLogic>();
            DependencyManager.RegisterService<IProjectLogic, ProjectLogic>();
            DependencyManager.RegisterService<ITaggingLogic, TaggingLogic>();
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
