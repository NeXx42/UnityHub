using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;

namespace UI.Popups.ActiveDownloads;

public partial class Popup_ActiveDownloads_Download : UserControl, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;
    public string VersionName { get; set; } = "";

    public Popup_ActiveDownloads_Download()
    {
        InitializeComponent();

        btn_Stop.RegisterClick(StopDownload);
    }

    public void Draw(string versionName, DownloadStatus downloadStatus)
    {
        this.DataContext = downloadStatus;

        this.VersionName = versionName;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    private void StopDownload()
    {
        IEditorLogic logic = DependencyManager.GetService<IEditorLogic>()!;
        logic.StopActiveInstall(VersionName);
    }
}