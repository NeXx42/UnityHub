using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Controls;

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

    public ModalContainer setContainer { set => _ = value; }

    public Task Show(string header, string paragraph)
    {
        task = new TaskCompletionSource();

        this.Header = header;
        this.Paragraph = paragraph;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        return task.Task;
    }
}