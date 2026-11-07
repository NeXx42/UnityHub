using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public partial class ConfirmationBox : UserControl, IModal, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    public string Header { get; set; } = "";
    public string Body { get; set; } = "";

    private ReusableList<ButtonWrapper> buttons;
    private TaskCompletionSource<int?>? completeTask;

    public ConfirmationBox()
    {
        InitializeComponent();
        buttons = new ReusableList<ButtonWrapper>(cont_Btns);
    }

    public ModalContainer? container { get; set; }

    public Task<int?> Show(string header, string msg, CancellationToken token, params IEnumerable<ConfirmationButton> btns)
    {
        this.Header = header;
        this.Body = msg;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

        completeTask?.SetCanceled();
        completeTask = new TaskCompletionSource<int?>().WithCancellationToken(token);

        buttons.Draw(btns, (btn, pos, dat) =>
        {
            btn.Label = dat.label;
            btn.Classes.Clear();

            if (!string.IsNullOrEmpty(dat.className))
                btn.Classes.Add(dat.className);

            btn.RegisterClick(() => completeTask?.SetResult(pos));
        });

        return completeTask.Task;
    }
}