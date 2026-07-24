using System.Threading.Tasks;
using Models.Enums;
using Models.Interfaces;

namespace UI.Pages.Settings.Pages.Common;

public interface ISettingsPageSetting
{
    public ISettingsPageSetting Init(ConfigEntry key);
    public Task Load(IConfigLogic configProvider);
}
