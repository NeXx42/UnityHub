using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Helpers;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Themes : UserControl, ISettingsPage
{
    private bool isActive = true;

    public SettingsPage_Themes()
    {
        InitializeComponent();

        inp_ThemeDropdown.SelectionChanged += (_, __) => UpdateTheme();
    }

    public UserControl getControl => this;

    public Task OnOpen()
    {
        isActive = false;

        string[] themes = ThemeHelper.GetThemes();
        int currentTheme = themes.ToList().IndexOf(ThemeHelper.currentThemeName);

        inp_ThemeDropdown.ItemsSource = themes;
        inp_ThemeDropdown.SelectedIndex = currentTheme == -1 ? 0 : currentTheme;

        isActive = true;
        return Task.CompletedTask;
    }

    private void UpdateTheme()
    {
        if (!isActive)
            return;

        ThemeHelper.ChangeTheme((string?)inp_ThemeDropdown.SelectedValue).Wrap();
    }
}