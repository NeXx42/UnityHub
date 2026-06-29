using System.Diagnostics;
using Models.Data;
using Models.Interfaces;

namespace Logic;

public static class ProjectLogic
{
    private static IDataRepository data => DependencyManager.dataRepo!;
    private static Dictionary<int, ProjectCache> cache = new Dictionary<int, ProjectCache>();

    public static async Task<ProjectCard[]> GetProjects()
    {
        return await data.GetProjectCards();
    }

    public static async Task<ProjectInfo> GetProjectInfo(int id)
    {
        ProjectCache proj = new();

        if (cache.TryGetValue(id, out proj))
        {
            if (proj.info != null)
                return proj.info;
        }

        ProjectInfo info = await data.GetProjectInfo(id);
        cache[id] = proj with
        {
            info = info
        };

        return info;
    }

    public static async Task Launch(int id)
    {
        ProjectInfo info = await GetProjectInfo(id);

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "/home/matth/Unity/Hub/Editor/6000.6.0a3/Editor/Unity"
        };

        startInfo.ArgumentList.Add("-projectPath");
        startInfo.ArgumentList.Add(info.directory);

        Process process = new Process()
        {
            StartInfo = startInfo
        };

        process.Start();
    }

    private record struct ProjectCache
    {
        public ProjectCard? card;
        public ProjectInfo? info;
    }
}
