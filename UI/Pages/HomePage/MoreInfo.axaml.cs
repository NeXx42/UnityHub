using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using UI.Helpers;

namespace UI.Pages.HomePage;

public partial class MoreInfo : UserControl
{
    public ProjectInfo? info { get; private set; }

    public MoreInfo()
    {
        InitializeComponent();
        btn_OpenProject.Click += (_, __) => _ = ProjectLogic.Launch(info!.id);
    }

    public async Task Show(int id)
    {
        info = await ProjectLogic.GetProjectInfo(id);
        DataContext = info;

        img.Source = await IconFetcher.GetImage(info.iconUrl);
    }
}