using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;

namespace UI.Modals;

public partial class EditorInstallerModal_Entry : UserControl
{
    private Func<Task>? onClick;

    public EditorInstallerModal_Entry()
    {
        InitializeComponent();
        btn_Install.RegisterClick(async () => await (onClick?.Invoke() ?? Task.CompletedTask));
    }

    public void Draw(EditorInfo info, Func<EditorInfo, Task> startInstall)
    {
        onClick = () => startInstall.Invoke(info);
        this.DataContext = info;
    }
}