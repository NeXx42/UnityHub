using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using UI.Controls;
using UI.Helpers;

namespace UI;

public partial class MainWindow : Window
{
    public static MainWindow? instance;
    private Stack<ModalContainer> activeModals;

    public MainWindow()
    {
        InitializeComponent();

        instance = this;
        activeModals = new Stack<ModalContainer>();



        try
        {
            _ = el_Sidebar.Init(page_HomePage);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Top level error, {e.Message}");
        }
    }

    public static T ShowModal<T>(out int pos) where T : UserControl
    {
        pos = instance!.activeModals.Count;

        ModalContainer container = new ModalContainer();

        instance.activeModals.Push(container);
        instance.cont_Modals.Children.Add(container);

        return container.ShowModal<T>(pos, CloseModal);
    }

    public static void CloseModal(int pos)
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
}