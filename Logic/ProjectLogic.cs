using System.Diagnostics;
using System.Text.Json;
using Models;
using Models.Data;
using Models.Interfaces;

namespace Logic;

public class ProjectLogic : IProjectLogic
{
    private record struct ProjectCache
    {
        public ProjectCard? card;
        public ProjectInfo? info;
    }

    private IDataRepository data => DependencyManager.GetService<IDataRepository>()!;
    private Dictionary<int, ProjectCache> cache = new Dictionary<int, ProjectCache>();

    public async Task Migrate()
    {
        string dirtyFile = Path.Combine(GlobalConfig.getDataFolder, "dirty");

        if (!File.Exists(dirtyFile))
            return;

        List<int> iconsToUpdate = new List<int>();
        string[] changes = File.ReadAllLines(dirtyFile);

        foreach (string change in changes)
        {
            string[] pair = change.Split(":");

            if (pair.Length > 1 && int.TryParse(pair[0], out int projectId))
            {
                iconsToUpdate.Add(projectId);
            }
        }

        File.Delete(dirtyFile);

        // may update more?
        await data.Migrate(iconsToUpdate);
    }

    public async Task<(ProjectCard[], int total)> Search(ProjectSearch search)
    {
        (int[] results, int total) = await data.Search(search);

        List<int> missingCardIds = new List<int>();
        List<ProjectCard> cards = new List<ProjectCard>();

        ProjectCache infoCache;

        foreach (int card in results)
        {
            if (cache.TryGetValue(card, out infoCache) && infoCache.card != null)
            {
                cards.Add(infoCache.card);
                continue;
            }

            missingCardIds.Add(card);
        }

        if (missingCardIds.Count > 0)
        {
            ProjectCard[] missingCards = await data.GetCardInfo(missingCardIds);

            foreach (ProjectCard card in missingCards)
            {
                if (!cache.TryGetValue(card.id, out infoCache))
                    infoCache = new ProjectCache();

                cache[card.id] = infoCache with
                {
                    card = card
                };

                cards.Add(card);
            }
        }

        return (cards.ToArray(), total);
    }

    public async Task<ProjectInfo> GetProjectInfo(int id)
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

    public async Task BrowseTo(int id) => await BrowseTo(await GetProjectInfo(id));
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

    public async Task<ProjectInfo[]> TryToUpload(string[] folders)
    {
        List<ProjectInfo> potentialCards = new List<ProjectInfo>();

        foreach (string folder in folders)
        {
            if (!ValidateFolder(folder))
                continue;

            DirectoryInfo dirInfo = new DirectoryInfo(folder);

            ProjectInfo card = new ProjectInfo()
            {
                id = -1,
                directory = folder,
                name = dirInfo.Name
            };

            await DependencyManager.GetService<EditorLogic>()!.DeriveProjectInfo(card);
            potentialCards.Add(card);
        }

        return potentialCards.ToArray();

        bool ValidateFolder(string folderName)
        {
            if (!folderName.EndsWith("/"))
                folderName = folderName + "/";

            var subDirs = Directory.GetDirectories(folderName)
                .Where(d => d.EndsWith("Assets") || d.EndsWith("ProjectSettings") || d.EndsWith("Packages"));

            return subDirs.Count() == 3;
        }
    }

    public async Task UploadCardsPrimitive(ProjectInfo[] cards)
    {
        await data.CreateCards(cards);
    }
}
