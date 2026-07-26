using System.Threading.Tasks;
using Models.Enums;
using Models.Interfaces;

namespace UI.Pages.Settings.Pages.Common;

public interface ISettingsPageSetting
{
    public ISettingsPageSetting Init(ConfigEntry key, bool supportWindows = true, bool supportLinux = true);
    public Task Load(IConfigLogic configProvider);
}
