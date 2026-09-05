using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Models.Helpers;
using UI.Pages.Settings;

namespace UnityHub.UI.Pages.Settings.Pages;

public partial class SettingsPage_Logs : UserControl, ISettingsPage
{
    public SettingsPage_Logs()
    {
        InitializeComponent();
    }

    public UserControl getControl => this;

    public async Task OnOpen()
    {
        List<string?> lines = await LoggingHelper.ReadLog();
        StringBuilder sb = new StringBuilder();

        foreach (string? line in lines)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            sb.AppendLine(line);
        }

        lbl.Text = sb.ToString();
    }
}