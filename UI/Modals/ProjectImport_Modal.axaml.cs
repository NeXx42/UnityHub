using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Modals.ProjectImport;

namespace UI.Modals;

public partial class ProjectImport_Modal : UserControl, IModal
{
    private TaskCompletionSource? task;
    private ReusableList<ProjectImport_Entry> importList;

    public ProjectImport_Modal()
    {
        InitializeComponent();

        importList = new ReusableList<ProjectImport_Entry>(cont_Projects);

        btn_Import.RegisterClick(Import);
        btn_Cancel.RegisterClick(() => task?.SetCanceled());
    }

    public ModalContainer setContainer { set => _ = value; }
    public bool canDismiss => true;

    public Task Show(IEnumerable<string> projects)
    {
        task = new TaskCompletionSource();

        importList.DrawAsync(
            () => DependencyManager.GetService<IProjectLogic>()!.VerifyProjectPrimative(projects),
            (ui, pos, dat) => ui.Draw(dat, pos)
        ).Wrap();

        return task.Task;
    }

    private async Task Import()
    {
        List<ProjectInfo> toImport = new(importList.getElementCount);

        foreach (ProjectImport_Entry ui in importList)
        {
            ProjectInfo? entry = ui.GetImport();

            if (entry != null)
                toImport.Add(entry);
        }

        await DependencyManager.ui!.LoadProgressive("Importing",
            new LoadRequest("Importing", (_) => DependencyManager.GetService<IProjectLogic>()!.UploadCardsPrimitive(toImport)));
        task?.SetResult();
    }
}