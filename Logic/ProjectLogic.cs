using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Models;
using Models.Data;
using Models.Enums;
using Models.Helpers;
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

        await Parallel.ForEachAsync(changes, async (change, token) =>
        {
            string[] pair = change.Split(":");

            if (pair.Length > 1 && int.TryParse(pair[0], out int projectId))
            {
                ProjectInfo? info = await data.GetProjectInfo(projectId);

                if (info == null)
                    return;

                await DeriveProjectInfo(info, true).WhenAllProgressive(CancellationToken.None);
                info.iconUrl = Path.Combine(GlobalConfig.getDataFolder, projectId.ToString(), "icon.png");

                updates.Add(info);
            }
        });

        File.Delete(dirtyFile);

        // may update more?
        await data.UpdateProjectProperties(updates, [
            nameof(ProjectInfo.iconUrl),
            nameof(ProjectInfo.created),
            nameof(ProjectInfo.size),
        ]);
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
                name = dirInfo.Name,
                collectionId = (int)DefaultCollectionIds.InDevelopment
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

        await DeriveProjectInfo(info, true).WhenAllProgressive(CancellationToken.None);
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
            await DependencyManager.ui!.LoadProgressive("Deleting",
                new LoadRequest("Deleting files", DeleteFiles),
                new LoadRequest("Removing data", RemoveInfo)
            );

            callback?.Invoke(nameof(DeleteCard));
        }
        catch { }

        async Task DeleteFiles(CancellationToken token)
        {
            if (Directory.Exists(info.directory))
                Directory.Delete(info.directory, true);
        }

        async Task RemoveInfo(CancellationToken token)
        {
            await data.DeleteCard([info.id]);
        }
    }

    public Task UpdateProperties(ProjectInfo info, IEnumerable<string> props) => data!.UpdateProjectProperties(info, props);
    public Task UpdateProperties(IEnumerable<ProjectInfo> elements, IEnumerable<string> props) => data!.UpdateProjectProperties(elements, props);

    public LoadRequest[] DeriveProjectInfo(ProjectInfo info, bool overrideCache)
    {
        return [
            new LoadRequest("Creation time", GetCreationTime),
            new LoadRequest("Size", GetSize),
            new LoadRequest("Version", GetVersionInfo),
            new LoadRequest("Packages", GetPackageInfo),
        ];

        async Task GetCreationTime(CancellationToken token)
        {
            info.created ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        async Task GetSize(CancellationToken token)
        {
            if (overrideCache || !info.size.HasValue)
                info.size = GetFolderSizeParallel(info.directory);

            long GetFolderSizeParallel(string path)
            {
                long total = 0;

                void Walk(DirectoryInfo dir)
                {
                    long localSize = 0;
                    var subDirs = new List<DirectoryInfo>();

                    try
                    {
                        foreach (var entry in dir.EnumerateFileSystemInfos())
                        {
                            if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint))
                                continue;

                            if (entry is FileInfo fi)
                                localSize += fi.Length;
                            else if (entry is DirectoryInfo di)
                                subDirs.Add(di);
                        }
                    }
                    catch (UnauthorizedAccessException) { return; }
                    catch (IOException) { return; }

                    Interlocked.Add(ref total, localSize);

                    if (subDirs.Count > 0)
                        Parallel.ForEach(subDirs, Walk);
                }

                Walk(new DirectoryInfo(path));
                return total;
            }
        }

        async Task GetVersionInfo(CancellationToken token)
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
        }

        async Task GetPackageInfo(CancellationToken token)
        {
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
}
