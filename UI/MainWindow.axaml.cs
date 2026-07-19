using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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
        MessageBox msg = ShowModal<MessageBox>(out int pos);
        await msg.Show(header, paragraph);

        await CloseModal(pos);
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

    public async Task<Exception?> LoadProgressive(string header, params LoadRequest[] reqs)
    {
        LoadingModal msg = ShowModal<LoadingModal>(out int pos);
        Exception? error = await msg.LoadProgressive(header, reqs);

        await CloseModal(pos);
        return error;
    }
}