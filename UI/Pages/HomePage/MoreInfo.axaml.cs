using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;

namespace UI.Pages.HomePage;

public partial class MoreInfo : UserControl
{
    public ProjectInfo? info { get; private set; }

    public MoreInfo()
    {
        InitializeComponent();
    }

    public async Task Show(int id)
    {
        info = await ProjectLogic.GetProjectInfo(id);
        DataContext = info;
    }
}