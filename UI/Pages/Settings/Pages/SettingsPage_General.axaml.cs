using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Enums;
using Models.Helpers;
using Models.Interfaces;
using UI.Helpers;
using UI.Pages.Settings.Pages.Common;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_General : UserControl, ISettingsPage
{
    public UserControl getControl => this;
    private ISettingsPageSetting[] settings;

    public SettingsPage_General()
    {
        InitializeComponent();

        btn_Projects_DeriveMissingMetadata.RegisterClick(DeriveMissingMetadata);

        settings = [
            setting_EditorCommand.Init(ConfigEntry.IDECommand),
            setting_TerminalCommand.Init(ConfigEntry.TerminalCommand)
        ];
    }


    public Task OnOpen()
    {
        IConfigLogic logic = DependencyManager.GetService<IConfigLogic>()!;
        Task.WhenAll(settings.Select(s => s.Load(logic))).Wrap();


        return Task.CompletedTask;
    }

    private async Task DeriveMissingMetadata()
    {
        IProjectLogic projectLogic = DependencyManager.GetService<IProjectLogic>()!;

        (ProjectInfo[] projects, _) = await projectLogic.Search(new ProjectSearch() { page = 0, take = 0 });
        await DependencyManager.ui!.LoadProgressive("Deriving", projects.Select(Work));

        LoadRequest Work(ProjectInfo project)
        {
            return new LoadRequest(project.name, Interal);

            async Task Interal(CancellationToken token)
            {
                try
                {
                    Console.WriteLine("Starting - " + project.name);
                    await projectLogic.DeriveProjectInfo(project, false).WhenAllProgressive(token);
                    Console.WriteLine("Done - " + project.name);
                }
                catch
                {
                    Console.WriteLine("Failed to derive");
                }
            }
        }

        await projectLogic.UpdateProperties(projects, [
            nameof(ProjectInfo.created),
            nameof(ProjectInfo.size),
            nameof(ProjectInfo.packages),
            nameof(ProjectInfo.version),
            nameof(ProjectInfo.renderPipeline),
        ]);
    }
}