using Models.Data;

namespace Models.Interfaces;

public interface IPlugin
{
    public Task Register();
    public LoadRequest[] OnLaunch();

    public bool RequiresAuthentication();
    public Task RequestSettingsOrAuth();
}
