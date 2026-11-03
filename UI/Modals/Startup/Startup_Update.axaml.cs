using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Interfaces;

namespace UnityHub.UI.Modals.Startup;

public partial class Startup_Update : UserControl
{
    public TaskCompletionSource task;

    public Startup_Update()
    {
        task = new TaskCompletionSource();
        InitializeComponent();

        btn_Ignore.RegisterClick(Ignore);
        btn_Update.RegisterClick(Update);
    }

    public Task Handle(string msg)
    {
        lbl_UpdateMessage.Content = msg;
        return task.Task;
    }

    private async Task Ignore()
    {
        IVersionLogic versionLogic = DependencyManager.GetService<IVersionLogic>()!;
        await DependencyManager.GetService<IConfigLogic>()!.Set(Models.Enums.ConfigEntry.IgnoredUpdate, versionLogic.getLatestVersion, true);

        task.SetResult();
    }

    private async Task Update()
    {

        task.SetResult();
    }
}