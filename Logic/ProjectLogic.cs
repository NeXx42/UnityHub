using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
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
        DependencyManager.GetService<ITaggingLogic>()!.RegisterCallback(OnTagginSituationChange);

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

    public async Task<ProjectInfo?> GetProjectInfo(int? id)
    {
        if (id == null)
            return null;

        if (cache.TryGetValue(id.Value, out ProjectInfo? proj))
        {
            return proj;
        }

        ProjectInfo? info = await data.GetProjectInfo(id.Value);
        cache[id.Value] = info;

        return info;
    }

    public async void BrowseTo(ProjectInfo info)
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

    public async Task OpenIDE(ProjectInfo info)
    {
        string? command = await DependencyManager.GetService<IConfigLogic>()!.Get<string?>(ConfigEntry.IDECommand, null);

        if (string.IsNullOrEmpty(command))
        {
            await DependencyManager.ui!.ShowMessageBox("No IDE Command", "Failed to open ide because there is no ide command set.\nThis can be done in the settings");
            return;
        }

        string[] parts = command.Split(" ");

        if (parts.Length == 0)
            parts = [command];

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = parts[0],
            UseShellExecute = false,
        };

        for (int i = 1; i < parts.Length; i++)
            if (parts[i].Equals("$"))
                startInfo.ArgumentList.Add(info.directory);
            else
                startInfo.ArgumentList.Add(parts[i]);

        new Process()
        {
            StartInfo = startInfo
        }.Start();
    }

    public async Task BrowseTerminal(ProjectInfo info)
    {
        string? command = await DependencyManager.GetService<IConfigLogic>()!.Get<string?>(ConfigEntry.TerminalCommand, null);

        if (GlobalConfig.isOnLinux && string.IsNullOrEmpty(command))
        {
            await DependencyManager.ui!.ShowMessageBox("No Terminal Command", "Failed to open the terminal because on linux the terminal couldnt be determined.\nSet one in the settings.");
            return;
        }

        new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = command ?? "cmd.exe",
                WorkingDirectory = info.directory,
                UseShellExecute = true,
            }
        }.Start();
    }

    public LoadRequest[] DuplicateProject(ProjectInfo info, string newName, string newDir)
    {
        if (Directory.Exists(newDir))
            throw new Exception($"Path ({newDir} already exists).");

        if (!Directory.Exists(info.directory))
            throw new Exception($"Source project doesnt exist at the directory {info.directory}.");

        return [
            CopyFolderAndDescendants(info.directory, newDir, true),
            new LoadRequest("Saving", CreateNewEntry)
        ];

        async Task CreateNewEntry(CancellationToken token)
        {
            ProjectInfo newInfo = new ProjectInfo()
            {
                collectionId = info.id,
                directory = newDir,
                name = newName,
                id = -1,
                size = info.size,
                favourited = false,
                notes = info.notes,
                packages = info.packages,
                renderPipeline = info.renderPipeline,
                tags = info.tags,
                version = info.version,
            };

            await UploadCardsPrimitive([newInfo]);
        }
    }

    public LoadRequest[] MoveProject(ProjectInfo info, string newDir)
    {
        if (Directory.Exists(newDir))
            throw new Exception($"Path '{newDir}' already exists.");

        if (!Directory.Exists(info.directory))
            throw new Exception($"Source project doesnt exist at the directory '{info.directory}'.");

        return [
            new LoadRequest("Moving", Move),
            new LoadRequest("Saving", UpdateEntry)
        ];

        async Task Move(IProgress<float> progress, CancellationToken token)
        {
            try
            {
                Directory.Move(info.directory, newDir);
            }
            catch (IOException ex) when (ex.Message.Contains("Invalid cross-device link"))
            {
                await CopyFolderAndDescendants(info.directory, newDir, false).Run(token, progress);
            }
        }

        async Task UpdateEntry(CancellationToken token)
        {
            info.directory = newDir;
            await UpdateProperties(info, [nameof(ProjectInfo.directory)]);
            cache.Remove(info.id);
        }
    }

    private LoadRequest CopyFolderAndDescendants(string from, string to, bool copyOnly)
    {
        return new LoadRequest("Copying", Work);

        async Task Work(IProgress<float> progress, CancellationToken token)
        {
            CopyFiles(from, to);

            void CopyFiles(string existing, string destination)
            {
                Directory.CreateDirectory(destination);

                foreach (var file in Directory.GetFiles(existing))
                {
                    var destFile = Path.Combine(destination, Path.GetFileName(file));
                    File.Copy(file, destFile, overwrite: true);
                }

                foreach (var directory in Directory.GetDirectories(existing))
                {
                    var destSubDir = Path.Combine(destination, Path.GetFileName(directory));
                    CopyFiles(directory, destSubDir);
                }

                if (!copyOnly)
                    Directory.Delete(existing, recursive: true);
            }
        }
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
        Dictionary<string, int> res = await data.CreateCards(cards);

        foreach (ProjectInfo card in cards)
        {
            if (res.TryGetValue(card.directory, out int databaseId))
                card.id = databaseId;
            else
                Console.WriteLine($"Failed to create project - {card.directory}");
        }

        callback?.Invoke(nameof(UploadCardsPrimitive));
    }

    public Task<string[]> GetProjectVersions() => data.GetProjectVersions();

    public LoadRequest[] DeleteCard(ProjectInfo info)
    {
        return [
            new LoadRequest("Deleting files", DeleteFiles),
            new LoadRequest("Removing data", RemoveInfo)
        ];

        async Task DeleteFiles(CancellationToken token)
        {
            if (Directory.Exists(info.directory))
                Directory.Delete(info.directory, true);
        }

        async Task RemoveInfo(CancellationToken token)
        {
            await data.DeleteCard([info.id]);
            callback?.Invoke(nameof(DeleteCard));
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

    public async Task<bool> TrySwitchVersion(ProjectInfo info, string to)
    {
        if (info.version?.Equals(to) ?? false)
            return false;

        if (await DependencyManager.ui!.ShowConfirmationBox(
            "Convert version",
            $"Are you sure you want to update the version to {to}?",
            new ConfirmationButton()
            {
                label = "Cancel"
            },
            new ConfirmationButton()
            {
                label = "Change",
                className = "Primary"
            }
        ) != 1)
            return false;

        info.version = to;

        IEditorLogic editor = DependencyManager.GetService<IEditorLogic>()!;

        await editor.LaunchProject(info);
        await UpdateProperties(info, [nameof(ProjectInfo.version)]);

        return true;
    }

    private void OnTagginSituationChange(int? id, string change)
    {
        if (!id.HasValue)
            return;

        switch (change)
        {
            case nameof(ITaggingLogic.DeleteCollection):
                int[] toDecache = cache.Values.Where(v => v != null && v.collectionId == id).Select(v => v!.id).ToArray();

                foreach (int i in toDecache)
                    cache.Remove(i);
                break;
        }
    }
}
