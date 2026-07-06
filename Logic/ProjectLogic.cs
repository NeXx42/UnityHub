using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Models;
using Models.Data;
using Models.Interfaces;

namespace Logic;

public class ProjectLogic : IProjectLogic
{
    private Action<string>? callback;

    private IDataRepository data => DependencyManager.GetService<IDataRepository>()!;
    private Dictionary<int, ProjectInfo?> cache = new Dictionary<int, ProjectInfo?>();

    public void RegisterCallback(Action<string> callback)
    {
        this.callback += callback;
    }

    public async Task Migrate()
    {
        string dirtyFile = Path.Combine(GlobalConfig.getDataFolder, "dirty");

        if (!File.Exists(dirtyFile))
            return;

        ConcurrentBag<ProjectInfo> updates = new();
        string[] changes = File.ReadAllLines(dirtyFile);

        IEditorLogic deriverisionLogic = DependencyManager.GetService<IEditorLogic>()!;

        await Parallel.ForEachAsync(changes, async (change, token) =>
        {
            string[] pair = change.Split(":");

            if (pair.Length > 1 && int.TryParse(pair[0], out int projectId))
            {
                ProjectInfo? info = await data.GetProjectInfo(projectId);

                if (info == null)
                    return;

                await deriverisionLogic.DeriveProjectInfo(info);
                info.iconUrl = Path.Combine(GlobalConfig.getDataFolder, projectId.ToString(), "icon.png");

                updates.Add(info);
            }
        });

        File.Delete(dirtyFile);

        // may update more?
        await data.Migrate(updates);
    }

    public async Task<(ProjectInfo[], int total)> Search(ProjectSearch search)
    {
        (int[] results, int total) = await data.Search(search);

        List<int> missingCardIds = new List<int>();
        List<ProjectInfo> cards = new List<ProjectInfo>();

        foreach (int card in results)
        {
            if (cache.TryGetValue(card, out ProjectInfo? cachedItem) && cachedItem != null)
            {
                cards.Add(cachedItem);
                continue;
            }

            missingCardIds.Add(card);
        }

        if (missingCardIds.Count > 0)
        {
            ProjectInfo[] missingCards = await data.GetProjectInfo(missingCardIds);

            foreach (ProjectInfo card in missingCards)
            {
                cache[card.id] = card;
                cards.Add(card);
            }
        }

        return (cards.ToArray(), total);
    }

    public async Task<ProjectInfo?> GetProjectInfo(int id)
    {
        if (cache.TryGetValue(id, out ProjectInfo? proj))
        {
            return proj;
        }

        ProjectInfo? info = await data.GetProjectInfo(id);
        cache[id] = info;

        return info;
    }

    public async Task BrowseTo(int id) => await BrowseTo((await GetProjectInfo(id))!);
    public async Task BrowseTo(ProjectInfo info)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "xdg-open",
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add(info.directory);

        Process process = new Process()
        {
            StartInfo = startInfo
        };

        process.Start();
    }

    public async Task<ProjectInfo[]> VerifyProjectPrimative(IEnumerable<string> folders)
    {
        List<ProjectInfo> potentialCards = new List<ProjectInfo>();

        foreach (string folder in folders)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folder);
            ProjectInfo card = new ProjectInfo()
            {
                id = -1,
                directory = folder,
                name = dirInfo.Name
            };

            potentialCards.Add(card);
        }

        return potentialCards.ToArray();
    }

    public async Task<ProjectInfo?> VerifyProjectPrimative(ProjectInfo info)
    {
        if (!ValidateFolder(info.directory))
            return null;

        DirectoryInfo dirInfo = new DirectoryInfo(info.directory);

        await DependencyManager.GetService<IEditorLogic>()!.DeriveProjectInfo(info);
        return info;

        bool ValidateFolder(string folderName)
        {
            if (!folderName.EndsWith("/"))
                folderName = folderName + "/";

            var subDirs = Directory.GetDirectories(folderName)
                .Where(d => d.EndsWith("Assets") || d.EndsWith("ProjectSettings") || d.EndsWith("Packages"));

            return subDirs.Count() == 3;
        }
    }

    public async Task UploadCardsPrimitive(IEnumerable<ProjectInfo> cards)
    {
        await data.CreateCards(cards);
        callback?.Invoke(nameof(UploadCardsPrimitive));
    }

    public Task<string[]> GetProjectVersions() => data.GetProjectVersions();

    public async Task DeleteCard(ProjectInfo info)
    {
        try
        {
            if (Directory.Exists(info.directory))
                Directory.Delete(info.directory, true);

            await data.DeleteCard([info.id]);
            callback?.Invoke(nameof(DeleteCard));
        }
        catch { }
    }
}
