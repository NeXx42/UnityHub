using Models.Data;

namespace Models.Interfaces;

public interface IUILinker
{
    public Task ShowMessageBox(string msg, string paragraph);
    public Task ShowMessageBox(Exception e);

    public Task RequestVersionInstall(string? version);

    public Task<int?> ShowConfirmationBox(string msg, string paragraph, params IEnumerable<ConfirmationButton> btns);

    public Task<Exception?> LoadProgressive(string header, params IEnumerable<LoadRequest> tasks);
}
