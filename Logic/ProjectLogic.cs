using Models.Data;
using Models.Interfaces;

namespace Logic;

public static class ProjectLogic
{
    private static IDataRepository data => DependencyManager.dataRepo!;

    public static async Task<ProjectCard[]> GetProjects()
    {
        return await data.GetProjectCards();
    }

    public static async Task<ProjectInfo> GetProjectInfo(int id)
    {
        return await data.GetProjectInfo(id);
    }
}
