using System;
using System.Threading.Tasks;
using Avalonia;
using Data.DataRepos;
using Data_Sqlite;
using Logic;
using Models.Interfaces;
using UI.Helpers;

namespace UI;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Setup().Wrap();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        async Task Setup()
        {
            await DependencyManager.RegisterService<IDataRepository, SqliteDataRepo>(repo => repo.Setup());

            DependencyManager.RegisterService<IEditorLogic, EditorLogic>();
            await DependencyManager.RegisterService<IProjectLogic, ProjectLogic>(logic => logic.Migrate());
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
