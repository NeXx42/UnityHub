using Models.Data;

namespace Models.Interfaces;

public interface IDataRepository
{
    public Task Setup();

    public Task<(int[], int)> Search(ProjectSearch search);

    public Task<ProjectCard[]> GetCardInfo(IEnumerable<int> ids);
    public Task<ProjectInfo> GetProjectInfo(int id);

    public Task CreateCard(ProjectInfo info);
    public Task CreateCards(IEnumerable<ProjectInfo> cards);
}
