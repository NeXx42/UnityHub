using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Models.Data;
using Models.Enums;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;

namespace UI.Popups;

public partial class Popup_Sort : UserControl, IPopup
{
    private bool isAscending;
    private int selectedOption;

    private ProjectSearch? currentSearchRef;
    private TaskCompletionSource? openTask;

    private Func<ProjectOrder, Task>? onUpdateAction;
    private ReusableList<ButtonWrapper> options;

    public Popup_Sort()
    {
        InitializeComponent();
        options = new ReusableList<ButtonWrapper>(cont, () =>
        {
            ButtonWrapper btn = new ButtonWrapper();
            btn.Classes.Add("Transparent");

            return btn;
        });

        btn_Asc.RegisterClick(() => ChangeOrder(true));
        btn_Desc.RegisterClick(() => ChangeOrder(false));
    }

    public void Draw(ProjectSearch currentSearchRef, Func<ProjectOrder, Task> onUpdateCallback)
    {
        this.currentSearchRef = currentSearchRef;
        onUpdateAction = onUpdateCallback;

        const string ascendingSuffix = "Asc";
        string[] names = Enum.GetNames<ProjectOrder>()
            .Where(e => e.EndsWith(ascendingSuffix))
            .Select(e => e.Substring(0, e.Length - ascendingSuffix.Length))
            .ToArray();

        options.Draw(names, (ui, pos, v) =>
        {
            ui.Label = v;
            ui.RegisterClick(() => SelectOption(pos));
        });
    }

    public Task Show()
    {
        openTask = new TaskCompletionSource();

        ProjectOrder order = currentSearchRef?.order ?? ProjectOrder.NameAsc;
        selectedOption = (int)Math.Floor((int)order / 2f);
        isAscending = (int)order % 2 == 0;

        ToggleOrderButtons();
        UpdateButtonSelection();

        return openTask.Task;
    }

    private async Task SelectOption(int pos)
    {
        selectedOption = pos;
        UpdateButtonSelection();

        int optionIndex = pos * 2;

        if (!isAscending)
            optionIndex++;

        await (onUpdateAction?.Invoke((ProjectOrder)optionIndex) ?? Task.CompletedTask);
    }

    private async Task ChangeOrder(bool isAscendingNew)
    {
        isAscending = isAscendingNew;
        ToggleOrderButtons();

        await SelectOption(selectedOption);
    }

    private void UpdateButtonSelection()
    {
        for (int i = 0; i < options.getElementCount; i++)
        {
            if (i == selectedOption)
                options[i].Classes.Add("Primary");
            else
                options[i].Classes.Remove("Primary");
        }
    }

    private void ToggleOrderButtons()
    {
        if (isAscending)
        {
            btn_Asc.Classes.Add("Primary");
            btn_Desc.Classes.Remove("Primary");
        }
        else
        {
            btn_Asc.Classes.Remove("Primary");
            btn_Desc.Classes.Add("Primary");
        }
    }
}