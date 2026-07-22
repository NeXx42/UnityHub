using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Data.DataRepos;
using Data_Sqlite;
using Logic;
using Logic.Editor;
using Models;
using Models.Interfaces;
using UI.Helpers;

namespace UI;

class Program
{
    private static List<IPlugin> loadedPlugins = new List<IPlugin>();

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

            if (GlobalConfig.isOnLinux)
            {
                DependencyManager.RegisterService<IEditorLogic, EditorLogic_Linux>();
            }
            else
            {
                DependencyManager.RegisterService<IEditorLogic, EditorLogic_Windows>();
            }

            DependencyManager.RegisterService<IConfigLogic, ConfigLogic>();
            DependencyManager.RegisterService<ITaggingLogic, TaggingLogic>();
            await DependencyManager.RegisterService<IProjectLogic, ProjectLogic>(logic => logic.Migrate());

            await LoadPlugins();
        }
    }

    private static async Task LoadPlugins()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Plugins");

        if (Directory.Exists(root))
        {
            string[] dlls = Directory.GetFiles(root, "*.dll", SearchOption.AllDirectories);

            foreach (string dll in dlls)
            {
                Assembly assembly = Assembly.LoadFrom(dll);

                try
                {
                    var entryPointType = assembly.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                    if (entryPointType == null)
                        continue;

                    IPlugin plugin = (IPlugin)Activator.CreateInstance(entryPointType)!;

                    await plugin.Register();
                    loadedPlugins.Add(plugin);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to load plugin\n{e.Message}");
                }
            }
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
