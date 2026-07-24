using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Modals;

namespace UI;

public interface IPage
{
    public Task<Control> Show();
    public Task Close();
}

public enum PageNames
{
    None,
    Home,
    Settings
}

public partial class MainWindow : Window, IUILinker
{
    public static MainWindow? instance;

    private PageNames activePage = PageNames.None;
    private Dictionary<PageNames, IPage> pages;

    private Stack<ModalContainer> activeModals;

    public MainWindow()
    {
        instance = this;
        ThemeHelper.Startup().Wrap();

        InitializeComponent();

        DependencyManager.Init(this);

        pages = new Dictionary<PageNames, IPage>()
        {
            { PageNames.Home, page_HomePage },
            { PageNames.Settings, page_Settings },
        };

        activeModals = new Stack<ModalContainer>();

        RequestPage(PageNames.Home).Wrap();

    }

    public static T ShowModal<T>(out int pos) where T : UserControl, IModal
    {
        pos = instance!.activeModals.Count;

        ModalContainer container = new ModalContainer();

        instance.activeModals.Push(container);
        instance.cont_Modals.Children.Add(container);

        return container.ShowModal<T>(pos, CloseModal);
    }

    public static async Task ShowModalAndWait<T>(Func<T, Task> handler) where T : UserControl, IModal
    {
        T modal = ShowModal<T>(out int pos);

        try
        {
            await handler(modal);
        }
        catch (TaskCanceledException) { }

        await CloseModal(pos);
    }

    public static async Task<RES?> ShowModalAndWait<T, RES>(Func<T, Task<RES>> handler) where T : UserControl, IModal
    {
        T modal = ShowModal<T>(out int pos);
        RES? res = default;

        try
        {
            res = await handler(modal);
        }
        catch (TaskCanceledException) { }

        await CloseModal(pos);
        return res;
    }

    public static async Task CloseModal(int pos)
    {
        for (int i = instance!.activeModals.Count - 1; i >= pos; i--)
        {
            if (instance.activeModals.TryPop(out ModalContainer? container) && container != null)
            {
                instance.cont_Modals.Children.RemoveAt(i);
            }
        }
    }

    public static async Task<string[]?> OpenFoldersDialog(string title)
    {
        var res = await instance!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = true,
            Title = title
        });

        if (res.Count == 0)
            return null;

        return res.Select(x => x.Path.LocalPath).ToArray();
    }

    public static async Task<string?> OpenFolderDialog(string title)
    {
        var res = await OpenFoldersDialog(title);
        return res != null ? res[0] : null;
    }

    public async Task ShowMessageBox(string header, string paragraph)
    {
        await ShowModalAndWait<MessageBox>(async m =>
        {
            await m.Show(header, paragraph);
        });
    }

    public async Task ShowMessageBox(Exception e)
    {
        await ShowModalAndWait<MessageBox>(async m =>
        {
            await m.Show(e);
        });
    }

    public static async Task RequestPage(PageNames desired)
    {
        if (desired == instance!.activePage)
            return;

        instance.activePage = desired;

        foreach (KeyValuePair<PageNames, IPage> page in instance.pages)
        {
            if (page.Key != desired)
            {
                await page.Value.Close();
                continue;
            }

            Control sidebar = await page.Value.Show();
            instance.el_Sidebar.Draw(desired, sidebar);
        }
    }

    public async Task<int?> ShowConfirmationBox(string header, string paragraph, params IEnumerable<ConfirmationButton> btns)
    {
        ConfirmationBox box = ShowModal<ConfirmationBox>(out int pos);
        int? res = await box.Show(header, paragraph, btns);

        await CloseModal(pos);
        return res;
    }

    public async Task<Exception?> LoadProgressive(string header, params IEnumerable<LoadRequest> tasks)
    {
        Exception? e = null;

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await ShowModalAndWait<LoadingModal>(async msg =>
            {
                e = await msg.LoadProgressive(header, tasks);
            });
        });

        return e;
    }

    public static void ClearFocus()
    {
        instance!.FocusManager?.Focus(null);
    }

    public async Task RequestVersionInstall(string? version)
    {
        if (string.IsNullOrEmpty(version))
        {
            await ShowMessageBox("Invalid version", $"Cant open installer because the selected version ({version}) is invalid");
            return;
        }

        IEditorLogic editorLogic = DependencyManager.GetService<IEditorLogic>()!;
        EditorInfo? metadata = await editorLogic.GetEditorMetadata(version);

        if (metadata == null)
        {
            await ShowMessageBox("Invalid version", $"Failed to find the metadata for the desired version {version}");
            return;
        }

        await ShowModalAndWait<EditorManagerModal>(async m => await m.Open(metadata));
    }
}