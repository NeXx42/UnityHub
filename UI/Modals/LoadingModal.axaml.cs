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

    public string percentage { get; set; } = "";

    public LoadingModal()
    {
        InitializeComponent();
    }

    public ModalContainer? container { get; set; }

    public async Task<Exception?> LoadProgressive(string header, params IEnumerable<LoadRequest> reqs)
    {
        this.header = header;
        this.msg = string.Empty;

        inp_Progress.Minimum = 0;
        inp_Progress.Maximum = (reqs.Count() - 1) * 100;
        inp_Progress.Value = 0;

        int reqProgress = 0;
        float subProgress = 0;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        CancellationTokenSource token = new CancellationTokenSource();

        foreach (var task in reqs)
        {
            msg = task.msg;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(msg)));

            Progress<float> secondaryProgress = new Progress<float>(v =>
            {
                subProgress = v;
                RedrawProgress();
            });
            Exception? e = await task.Run(token.Token, secondaryProgress);

            if (e != null)
                return e;

            reqProgress++;
            subProgress = 0;

            RedrawProgress();
        }

        return null;

        void RedrawProgress()
        {
            inp_Progress.Value = (100 * reqProgress) + subProgress;
            percentage = $"{Math.Round((inp_Progress.Value / inp_Progress.Maximum) * 100)}%";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(percentage)));
        }
    }
}