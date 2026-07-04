using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using UI.Helpers;

namespace UI.Popups;

public partial class Popup_FilterGroup : UserControl
{
    public string Header { get; set; } = "test";

    private ReusableList<CheckBox> mutualList;
    private ReusableList<RadioButton> nonMutualList;

    private Action? onClick;
    private Func<IEnumerable<int>, Task>? selectionUpdate;

    private HashSet<int> selectedOptions = new HashSet<int>();

    public bool toggle
    {
        set
        {
            cont_Container.IsVisible = value;
        }
    }

    public Popup_FilterGroup()
    {
        InitializeComponent();

        mutualList = new ReusableList<CheckBox>(cont);
        nonMutualList = new ReusableList<RadioButton>(cont);

        btn.PointerPressed += (_, __) => onClick?.Invoke();
    }

    public Popup_FilterGroup Init(string header, IEnumerable<string> options, IEnumerable<int> selectedIndexes, bool mutuallyExclusive, Action onToggle, Func<IEnumerable<int>, Task> selectionUpdate)
    {
        this.Header = header;

        this.onClick = onToggle;
        this.selectionUpdate = selectionUpdate;

        if (mutuallyExclusive)
        {
            mutualList.Draw(options, (ui, pos, val) =>
            {
                ui.Content = val;

                ui.IsCheckedChanged -= (sender, _) => UpdateMutualOption(pos, ((CheckBox?)sender)?.IsChecked ?? false);
                ui.IsChecked = selectedIndexes.Contains(pos);

                ui.IsCheckedChanged += (sender, _) => UpdateMutualOption(pos, ((CheckBox?)sender)?.IsChecked ?? false);
            });
        }
        else
        {
            string groupName = Guid.NewGuid().ToString();
            int? selectedIndex = selectedIndexes.FirstOrDefault();

            nonMutualList.Draw(options, (ui, pos, val) =>
            {
                ui.Content = val;
                ui.GroupName = groupName;

                ui.IsCheckedChanged -= (sender, _) => UpdateNonMutualOption(pos, ((RadioButton?)sender)?.IsChecked ?? false);
                ui.IsChecked = pos == selectedIndex;

                ui.IsCheckedChanged += (sender, _) => UpdateNonMutualOption(pos, ((RadioButton?)sender)?.IsChecked ?? false);
            });
        }

        return this;
    }

    private void UpdateMutualOption(int pos, bool isChecked)
    {
        if (isChecked)
        {
            if (!selectedOptions.Contains(pos))
                selectedOptions.Add(pos);
        }
        else
            selectedOptions.Remove(pos);

        selectionUpdate?.Invoke(selectedOptions);
    }

    private void UpdateNonMutualOption(int pos, bool isChecked)
    {
        if (isChecked)
        {
            selectedOptions = [pos];
            selectionUpdate?.Invoke(selectedOptions);
        }
    }
}