using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;

namespace UI.Modals.ProjectImport;

public partial class ProjectImport_Entry : UserControl
{
    public ProjectInfo? GetImport() => (inp_Checkbox.IsChecked ?? false) ? (ProjectInfo?)DataContext : null;

    public ProjectImport_Entry()
    {
        InitializeComponent();
    }

    public void Draw(ProjectInfo info, int pos)
    {

        if (pos % 2 == 0)
            root.Classes.Remove("Odd");
        else
            root.Classes.Add("Odd");


        this.DataContext = info;
        inp_Checkbox.IsChecked = true;
    }
}