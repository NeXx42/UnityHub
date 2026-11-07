using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public partial class MessageBox : UserControl, IModal, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    private TaskCompletionSource? task;

    public string? Header { get; set; } = "";
    public string? Paragraph { get; set; } = "";

    public MessageBox()
    {
        InitializeComponent();
        btn_ok.RegisterClick(() => task?.SetResult());
    }

    public ModalContainer? container { get; set; }

    public async Task Show(Exception e, CancellationToken cancellationToken)
        => await Show("Unexpeted error", e.Message, cancellationToken, true);

    public Task Show(string header, string paragraph, CancellationToken cancellationToken, bool isException = false)
    {
        if (isException)
        {
            Classes.Add("Error");
            btn_ok.Classes.Add("Negative");
        }

        img_Error.IsVisible = isException;
        task = new TaskCompletionSource().WithCancellationToken(cancellationToken);

        this.Header = header;
        this.Paragraph = paragraph;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        return task.Task;
    }
}