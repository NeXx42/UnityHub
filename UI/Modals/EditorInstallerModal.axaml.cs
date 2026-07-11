using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public partial class EditorInstallerModal : UserControl, IModal
{
    private ReusableList<EditorInstallerModal_Entry> entryList;

    public EditorInstallerModal()
    {
        InitializeComponent();
        entryList = new ReusableList<EditorInstallerModal_Entry>(entries);
    }

    public ModalContainer setContainer { set => _ = value; }

    public async Task Open()
    {
        EditorInfo[]? editors = await loadingBoundary.Load(GetPotentialInstalls);
        entryList.Draw(editors ?? [], (ui, _, dat) => ui.Draw(dat, StartInstall));
    }

    private async Task<EditorInfo[]> GetPotentialInstalls()
    {
        EditorInfo[] info = await DependencyManager.GetService<IEditorLogic>()!.GetEditorDownloads();
        return info;
    }

    private async Task StartInstall(EditorInfo version)
    {
        EditorModuleInstallerModal modal = MainWindow.ShowModal<EditorModuleInstallerModal>(out int pos);
        await modal.Show(version);
        await MainWindow.CloseModal(pos);
    }
}