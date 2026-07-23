using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Helpers;

namespace UI.Modals;

public partial class EditorManager_ModuleGroup : UserControl
{
    private ReusableList<EditorManager_Module> moduleList;

    public EditorManager_ModuleGroup()
    {
        InitializeComponent();

        moduleList = new ReusableList<EditorManager_Module>(cont);
    }

    public void Draw(string header, EditorInfo.Download.Module[] modules, HashSet<string> installedModules)
    {
        lbl.Content = header;
        moduleList.Draw(modules, (ui, i, dat) => ui.Draw(dat, i, installedModules.Contains(dat.id)));
    }

    public void AddSelectedModules(ref HashSet<string> modules)
    {
        foreach (EditorManager_Module ui in moduleList)
        {
            if (ui.IsSelected(out string moduleId))
                modules.Add(moduleId);
        }
    }
}