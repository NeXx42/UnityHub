using Models.Data;

namespace Models.Interfaces;

public interface IUILinker
{
    public Task ShowMessageBox(string msg, string paragraph);
    public Task<Exception?> LoadProgressive(string header, params LoadRequest[] tasks);
}
