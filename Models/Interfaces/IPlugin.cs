namespace Models.Interfaces;

public interface IPlugin
{
    public Task Register();
    public bool RequiresAuthentication();

    public Task RequestSettingsOrAuth();
}
