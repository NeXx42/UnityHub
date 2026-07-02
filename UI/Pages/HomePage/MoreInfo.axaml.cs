using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Helpers;

namespace UI.Pages.HomePage;

public partial class MoreInfo : UserControl
{
    public ProjectInfo? info { get; private set; }

    public MoreInfo()
    {
        InitializeComponent();

        btn_OpenProject.Click += (_, __) => _ = DependencyManager.GetService<IEditorLogic>()!.LaunchProject(info!);
        btn_OpenExplorer.Click += (_, __) => _ = DependencyManager.GetService<IProjectLogic>()!.BrowseTo(info!);
    }

    public async Task Show(int id)
    {
        info = await DependencyManager.GetService<IProjectLogic>()!.GetProjectInfo(id);
        DataContext = info;

        img.Source = await IconFetcher.GetImage(info.iconUrl);
    }
}