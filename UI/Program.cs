using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Data_Sqlite;
using Logic;
using Logic.Editor;
using Logic.Versioning;
using Models;
using Models.Helpers;
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
        LoggingHelper.ClearLog();
        LoggingHelper.Log("App startup");

        try
        {
            LoggingHelper.Log("Running setup");
            Setup().Wait();
            LoggingHelper.Log("Completed setup");

            LoggingHelper.Log("Starting avalonia");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            LoggingHelper.Log("Ending avalonia");
        }
        catch (Exception e)
        {
            LoggingHelper.LogError(e);
        }

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = (Exception)args.ExceptionObject;

            LoggingHelper.LogError(exception);
            DependencyManager.ui!.ShowMessageBox(exception!);
        };

        async Task Setup()
        {
            IEnumerable<AssemblyMetadataAttribute>? applicationMetadata = Assembly.GetEntryAssembly()?.GetCustomAttributes<AssemblyMetadataAttribute>();

            DependencyManager.RegisterService<IConfigLogic, ConfigLogic>();
            await DependencyManager.RegisterService<IDataRepository, SqliteDataRepo>(repo => repo.Setup());

            if (GlobalConfig.isOnLinux)
            {
                DependencyManager.RegisterService<IEditorLogic, EditorLogic_Linux>();
                DependencyManager.RegisterService<IVersionLogic, Versioning_AppImage>(applicationMetadata);
            }
            else
            {
                DependencyManager.RegisterService<IEditorLogic, EditorLogic_Windows>();
                DependencyManager.RegisterService<IVersionLogic, Versioning_Windows>(applicationMetadata);
            }

            DependencyManager.RegisterService<ITaggingLogic, TaggingLogic>();
            await DependencyManager.RegisterService<IProjectLogic, ProjectLogic>(logic => logic.Migrate());

            await PluginHelper.LoadPlugins();
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
