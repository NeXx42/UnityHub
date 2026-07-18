using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;

namespace UI.Modals;

public partial class CreateProjectModal_Package : UserControl
{
    private EditorInstallInfo.BuiltInPackage representing;

    public CreateProjectModal_Package()
    {
        InitializeComponent();
    }

    public void Draw(EditorInstallInfo.BuiltInPackage pkg)
    {
        this.DataContext = pkg;
        this.representing = pkg;
    }

    public void UpdateInPackageList(ref Dictionary<string, string> selectedPackageList)
    {
        inp_Select.IsChecked = selectedPackageList.ContainsKey(representing.name);
    }
}