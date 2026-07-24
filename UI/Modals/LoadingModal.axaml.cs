using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Controls;

namespace UI.Modals;

public partial class LoadingModal : UserControl, IModal, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    public string header { get; set; } = "";
    public string msg { get; set; } = "";

    public LoadingModal()
    {
        InitializeComponent();
    }

    public bool canDismiss => false;
    public ModalContainer setContainer { set => _ = value; }

    public async Task<Exception?> LoadProgressive(string header, params IEnumerable<LoadRequest> reqs)
    {
        this.header = header;
        this.msg = string.Empty;

        inp_Progress.Minimum = 0;
        inp_Progress.Maximum = reqs.Count() - 1;
        inp_Progress.Value = 0;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        CancellationTokenSource token = new CancellationTokenSource();

        foreach (var task in reqs)
        {
            msg = task.msg;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(msg)));

            Exception? e = await task.Run(token.Token);

            if (e != null)
                return e;

            inp_Progress.Value++;
        }

        return null;
    }
}