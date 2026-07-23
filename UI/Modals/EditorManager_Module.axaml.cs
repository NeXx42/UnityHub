using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;

namespace UI.Modals;

public partial class EditorManager_Module : UserControl
{
    private string? id;

    public EditorManager_Module()
    {
        this.DataContext = new EditorInfo.Download.Module() { id = "" };
        InitializeComponent();
    }

    public void Draw(EditorInfo.Download.Module module, int pos, bool isInstalled)
    {
        if (pos % 2 == 0)
            root.Classes.Remove("Odd");
        else
            root.Classes.Add("Odd");

        this.id = module.id;
        this.DataContext = module;

        inp_Checkbox.IsChecked = isInstalled;
        inp_Checkbox.IsEnabled = !isInstalled;
    }

    public bool IsSelected(out string id)
    {
        id = this.id!;
        return (inp_Checkbox.IsChecked ?? false) && inp_Checkbox.IsEnabled;
    }
}