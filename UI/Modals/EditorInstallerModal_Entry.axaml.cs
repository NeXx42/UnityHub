using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public partial class EditorInstallerModal_Entry : UserControl
{
    private ReusableList<CollectionItem> tagList;

    public EditorInstallerModal_Entry()
    {
        InitializeComponent();

        tagList = new ReusableList<CollectionItem>(cont_Tags);
    }

    public void Draw(EditorInfo info, int pos, Func<EditorInfo, Task> startInstall)
    {
        if (pos % 2 == 0)
            root.Classes.Remove("Odd");
        else
            root.Classes.Add("Odd");

        this.DataContext = info;

        if (info is EditorInstallInfo installedInfo)
        {
            btn_Download.IsEnabled = false;
            btn_Download.Classes.Remove("Primary");
        }
        else
        {
            btn_Download.IsEnabled = true;
            btn_Download.Classes.Add("Primary");
            btn_Download.RegisterClick(() => startInstall(info));
        }

        tagList.Draw(info.CreateTags(), (ui, _, dat) => ui.Init(dat));
    }
}