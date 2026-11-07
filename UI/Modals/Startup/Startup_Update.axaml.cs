using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Interfaces;
using UI.Helpers;

namespace UnityHub.UI.Modals.Startup;

public partial class Startup_Update : UserControl
{
    public TaskCompletionSource? task;

    public Startup_Update()
    {
        InitializeComponent();

        btn_Ignore.RegisterClick(Ignore);
        btn_Update.RegisterClick(Update);
    }

    public Task Handle(string msg, CancellationToken token)
    {
        lbl_UpdateMessage.Content = msg;
        task = new TaskCompletionSource(token).WithCancellationToken(token);

        return task.Task;
    }

    private async Task Ignore()
    {
        IVersionLogic versionLogic = DependencyManager.GetService<IVersionLogic>()!;
        await DependencyManager.GetService<IConfigLogic>()!.Set(Models.Enums.ConfigEntry.IgnoredUpdate, versionLogic.getLatestVersion, true);

        task!.SetResult();
    }

    private async Task Update()
    {
        task!.SetResult();
    }
}