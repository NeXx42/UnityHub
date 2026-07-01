using System.Diagnostics;
using System.Text.Json;
using Models.Data;
using Models.Interfaces;

namespace Logic;

public static class ProjectLogic
{
    private record struct ProjectCache
    {
        public ProjectCard? card;
        public ProjectInfo? info;
    }

    private static IDataRepository data => DependencyManager.dataRepo!;
    private static Dictionary<int, ProjectCache> cache = new Dictionary<int, ProjectCache>();

    public static async Task<ProjectCard[]> Search(ProjectSearch search)
    {
        (int[] results, _) = await data.Search(search);

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

        return cards.ToArray();
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

    public static async Task BrowseTo(int id) => await BrowseTo(await GetProjectInfo(id));
    public static async Task BrowseTo(ProjectInfo info)
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

    public static async Task<ProjectInfo[]> TryToUpload(string[] folders)
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

            await DeriveProjectInfo(card);
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

    public static async Task UploadCardsPrimitive(ProjectInfo[] cards)
    {
        await data.CreateCards(cards);
    }

    public static async Task DeriveProjectInfo(ProjectInfo info)
    {
        string versionFile = Path.Combine(info.directory, "ProjectSettings", "ProjectVersion.txt");

        if (File.Exists(versionFile))
        {
            using (StreamReader reader = new StreamReader(versionFile))
            {
                string? line = await reader.ReadLineAsync();

                if (!string.IsNullOrEmpty(line))
                {
                    string[] parts = line.Split(":");
                    info.version = parts[1].Replace(" ", "");
                }
            }
        }

        string manifestFile = Path.Combine(info.directory, "Packages", "manifest.json");

        if (File.Exists(manifestFile))
        {
            using (StreamReader reader = new StreamReader(manifestFile))
            {
                string manifestJson = await reader.ReadToEndAsync();
                JsonElement element = JsonDocument.Parse(manifestJson).RootElement.GetProperty("dependencies");

                info.packages = element.GetPropertyCount();

                if (element.TryGetProperty("com.unity.render-pipelines.universal", out _))
                    info.renderPipeline = RenderPipelineTypes.Universal_Render_Pipeline;
                else if (element.TryGetProperty("com.unity.render-pipelines.high-definition", out _))
                    info.renderPipeline = RenderPipelineTypes.High_Definition_Render_Pipeline;
                else
                    info.renderPipeline = RenderPipelineTypes.Built_In_Render_Pipeline;
            }
        }
    }
}
