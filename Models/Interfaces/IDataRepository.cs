using Models.Data;

namespace Models.Interfaces;

public interface IDataRepository
{
    public Task Setup();
    public Task<ProjectCard[]> GetProjectCards();
    public Task<ProjectInfo> GetProjectInfo(int id);

    public Task CreateCard(ProjectInfo info);
    public Task CreateCards(IEnumerable<ProjectInfo> cards);
}
