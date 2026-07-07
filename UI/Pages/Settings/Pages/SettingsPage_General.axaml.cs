using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_General : UserControl, ISettingsPage
{
    public UserControl getControl => this;

    public SettingsPage_General()
    {
        InitializeComponent();

        btn_Projects_DeriveMissingMetadata.RegisterClick(DeriveMissingMetadata);
    }


    public Task OnOpen()
    {
        return Task.CompletedTask;
    }

    private async Task DeriveMissingMetadata()
    {
        IProjectLogic projectLogic = DependencyManager.GetService<IProjectLogic>()!;
        IEditorLogic editorLogic = DependencyManager.GetService<IEditorLogic>()!;

        (ProjectInfo[] projects, _) = await projectLogic.Search(new ProjectSearch() { page = 0, take = 0 });
        Stopwatch timeoutLimiter = new Stopwatch();

        foreach (ProjectInfo project in projects)
        {
            timeoutLimiter.Start();

            try
            {
                Console.WriteLine("Starting - " + project.name);
                await editorLogic.DeriveProjectInfo(project);
                Console.WriteLine("Done - " + project.name);
            }
            catch
            {
                Console.WriteLine("Failed to derive");
            }

            timeoutLimiter.Reset();
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