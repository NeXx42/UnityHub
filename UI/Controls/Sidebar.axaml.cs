using System.Threading.Tasks;
using Avalonia.Controls;

namespace UI.Controls;

public interface ISidebarControl
{
    public bool setSelected { set; }
}

public partial class Sidebar : UserControl
{
    public Sidebar()
    {
        InitializeComponent();

        btn_Home.RegisterClick(() => MainWindow.RequestPage(PageNames.Home));
        btn_Settings.RegisterClick(() => MainWindow.RequestPage(PageNames.Settings));
        btn_Downloads.RegisterClick(() => MainWindow.RequestPage(PageNames.Settings));
    }

    public async Task Init()
    {

    }

    public void Draw(PageNames page, Control sidebar)
    {
        this.content.Content = sidebar;

        UpdateHighlight(btn_Home, page == PageNames.Home);
        UpdateHighlight(btn_Settings, page == PageNames.Settings);

        void UpdateHighlight(ButtonWrapper wrapper, bool isSelected)
        {
            if (isSelected)
            {
                wrapper.Classes.Add("Primary");
                wrapper.Classes.Remove("Transparent");
            }
            else
            {
                wrapper.Classes.Add("Transparent");
                wrapper.Classes.Remove("Primary");
            }
        }
    }
}