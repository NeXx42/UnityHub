using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Logic;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Modals;

namespace UI;

public partial class MainWindow : Window, IUILinker
{
    public static MainWindow? instance;
    private Stack<ModalContainer> activeModals;

    public MainWindow()
    {
        InitializeComponent();

        DependencyManager.Init(this);

        instance = this;
        activeModals = new Stack<ModalContainer>();

        el_Sidebar.Init(page_HomePage).Wrap();
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
}