using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Helpers;
using UI.Interfaces;
using UI.Popups.ActiveDownloads;

namespace UI.Popups;

public partial class Popup_ActiveDownloads : UserControl, IPopup
{
    private ReusableList<Popup_ActiveDownloads_Download> downloadList;

    public Popup_ActiveDownloads()
    {
        InitializeComponent();
        downloadList = new ReusableList<Popup_ActiveDownloads_Download>(cont);
    }

    public Task Show()
    {
        TaskCompletionSource task = new TaskCompletionSource();
        Draw();

        return task.Task;
    }

    private void Draw()
    {
        IEditorLogic logic = DependencyManager.GetService<IEditorLogic>()!;
        Dictionary<EditorInfo, DownloadStatus> activeDownloads = logic.GetActiveInstalls();

        downloadList.Draw(activeDownloads, (ui, _, dat) => ui.Draw(dat.Key.versionName, dat.Value));
    }
}