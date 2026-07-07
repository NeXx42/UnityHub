using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Models;
using Models.Data;
using Models.Interfaces;

namespace Logic;

public class EditorLogic : IEditorLogic
{
    public IDataRepository database => DependencyManager.GetService<IDataRepository>()!;
    public const string LINK_NAME = "com.nexx.unityhublink";

    private string[]? editorLocations;
    private Dictionary<int, ActiveInstances> activeInstances = new Dictionary<int, ActiveInstances>();

    public async Task<bool> IsVersionInstalled(string? version) => !string.IsNullOrEmpty(await GetEditorInstall(version));

    public async Task<string[]> GetEditorLocations(bool recache = false)
    {
        if (editorLocations == null || recache)
            editorLocations = await DependencyManager.GetService<IConfigLogic>()!.Get<string[]>(Models.Enums.ConfigEntry.EditorPath, []);

        return editorLocations;
    }

    public async Task<string?> GetEditorInstall(string? version)
    {
        if (string.IsNullOrEmpty(version))
            return null;

        string path;

        foreach (string root in await GetEditorLocations())
        {
            path = Path.Combine(root, version);

            if (Directory.Exists(path))
                return Path.Combine(path, "Editor", "Unity");
        }

        return null;
    }

    public async Task<string[]> GetInstalledEditorVersions()
    {
        HashSet<string> installs = new HashSet<string>();
        string[] dirs;

        foreach (string root in await GetEditorLocations())
        {
            dirs = GetEditorInstallerPerDir(root);

            foreach (string version in dirs)
                if (!installs.Contains(version))
                    installs.Add(version);
        }

        return installs.ToArray();
    }

    public async Task<EditorInstallInfo[]> GetInstalledEditorVersionsMoreInfo(CancellationToken token)
    {
        List<EditorInstallInfo> results = new();
        string[] versions;

        foreach (string root in await GetEditorLocations())
        {
            versions = GetEditorInstallerPerDir(root);
            results.AddRange(versions.Select(r => new EditorInstallInfo()
            {
                installLocation = root,
                versionName = r
            }));
        }

        await GetEditorInfoForVersions(results, token);
        await GetInstalledEditorInfoForVersions(results);

        return results.ToArray();
    }


    private async Task GetEditorInfoForVersions(IEnumerable<EditorInfo> allVersions, CancellationToken cancellationToken)
    {
        ConcurrentDictionary<string, EditorInfo> versions = new ConcurrentDictionary<string, EditorInfo>(allVersions.ToDictionary(v => v.versionName, v => v));
        Dictionary<string, string> versionInfoJson = await database.GetEditorInfo(versions.Keys);

        ConcurrentDictionary<string, string> newVersionInfo = new();

        foreach (KeyValuePair<string, EditorInfo> version in versions)
        {
            if (!versionInfoJson.ContainsKey(version.Key))
                newVersionInfo.AddOrUpdate(version.Key, string.Empty, (_, __) => string.Empty);
        }

        if (newVersionInfo.Count > 0)
        {
            SemaphoreSlim slim = new SemaphoreSlim(10); // rate limit
            Random r = new Random();

            using (HttpClient http = new HttpClient())
            {
                await Parallel.ForEachAsync(newVersionInfo, async (version, token) =>
                {
                    // if the request fails 10 time, idk
                    for (int i = 0; i < 10; i++)
                    {
                        if (await SendRequest())
                            break;
                    }

                    // returns true if capture what was needed
                    async Task<bool> SendRequest()
                    {
                        try
                        {
                            await slim.WaitAsync();
                            await Task.Delay(r.Next(10, 100));

                            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, $"https://services.api.unity.com/unity/editor/release/v1/releases?version={version.Key}");
                            HttpResponseMessage res = await http.SendAsync(msg);

                            if (res.StatusCode == HttpStatusCode.TooManyRequests)
                                return false;

                            res.EnsureSuccessStatusCode();

                            string json = await res.Content.ReadAsStringAsync();

                            if (string.IsNullOrEmpty(json))
                                throw new Exception("Failed to get content?");

                            newVersionInfo[version.Key] = json;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed fetch - " + e.Message);
                        }
                        finally
                        {
                            slim.Release();
                        }

                        return true;
                    }

                });
            }

            await database.SetEditorInfo(newVersionInfo.ToDictionary());

            foreach (KeyValuePair<string, string> i in newVersionInfo)
                versionInfoJson.Add(i.Key, i.Value);
        }

        await Parallel.ForEachAsync(versionInfoJson, async (KeyValuePair<string, string> json, CancellationToken token) =>
        {
            JsonDocument doc = JsonDocument.Parse(json.Value);

            foreach (JsonElement result in doc.RootElement.GetProperty("results").EnumerateArray())
            {
                EditorInfo info = versions[json.Key];

                info.stream = result.GetProperty("stream").GetString();

                if (result.TryGetProperty("label", out JsonElement lbl))
                    info.label = new EditorInfo.Label
                    {
                        description = lbl.GetProperty("description").GetString(),
                        labelText = lbl.GetProperty("labelText").GetString(),
                        colour = lbl.GetProperty("color").GetString(),
                        icon = lbl.GetProperty("icon").GetString(),
                    };

                if (result.TryGetProperty("downloads", out JsonElement downloads))
                    info.downloads = downloads.EnumerateArray().Select(download => new EditorInfo.Download
                    {
                        url = download.GetProperty("url").GetString(),
                        type = download.GetProperty("type").GetString(),
                        platform = download.GetProperty("platform").GetString(),
                        architecture = download.GetProperty("architecture").GetString(),

                        downloadSize = download.TryGetProperty("downloadSize", out JsonElement downloadSize) ? downloadSize.GetProperty("value").GetUInt64() : 0,
                        installSize = download.TryGetProperty("installSize", out JsonElement installSize) ? installSize.GetProperty("value").GetUInt64() : 0,

                        integrity = download.GetProperty("integrity").GetString(),

                    }).ToArray();

                break;
            }
        });
    }

    private async Task GetInstalledEditorInfoForVersions(IEnumerable<EditorInstallInfo> installs)
    {
        await Parallel.ForEachAsync(installs, async (el, token) =>
        {
            string moduleInfo = Path.Combine();

            //if (Directory.Exists())

        });
    }


    private string[] GetEditorInstallerPerDir(string root)
    {
        List<string> res = new List<string>();
        string[] dirs = Directory.GetDirectories(root);

        foreach (string dir in dirs)
        {
            string versionName = Path.GetFileName(dir)!;
            res.Add(versionName);
        }

        return res.ToArray();
    }

    public bool IsProjectRunning(int id) => activeInstances.ContainsKey(id);

    public async Task LaunchProject(int id) => await LaunchProject(await DependencyManager.GetService<IProjectLogic>()!.GetProjectInfo(id));
    public async Task LaunchProject(ProjectInfo? info)
    {
        if (info == null)
        {
            await DependencyManager.ui!.ShowMessageBox("Project is invalid", "Failed to launch the project because the id didn't correspond with a known entry.");
            return;
        }

        if (IsProjectRunning(info.id))
        {
            await DependencyManager.ui!.ShowMessageBox("Already running", "Failed to launch the project because the project is already open.");
            return;
        }

        if (!await IsVersionInstalled(info.version))
        {
            await DependencyManager.ui!.ShowMessageBox("Version not found", $"Failed to launch the project because the unity editor version {info.version} was not found.");
            return;
        }

        if (true)
        {
            await InjectLinker(info.directory, info.id);
        }

        info.lastOpened = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await database!.UpdateProjectProperties(info, [nameof(ProjectInfo.lastOpened)]);

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = await GetEditorInstall(info.version!)
        };

        startInfo.ArgumentList.Add("-projectPath");
        startInfo.ArgumentList.Add(info.directory);

        ActiveInstances instance = new ActiveInstances(startInfo);
        activeInstances.Add(info.id, instance);
    }

    public async Task DeriveProjectInfo(ProjectInfo info)
    {
        info.created ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        info.size ??= GetFolderSizeParallel(info.directory);

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

    private async Task InjectLinker(string targetRoot, int projectId)
    {
        string manifestFile = Path.Combine(targetRoot, "Packages", "manifest.json");

        if (!File.Exists(manifestFile))
        {
            Console.WriteLine("Failed to find manifest file? invalid project");
            return;
        }

        string handoverFile = Path.Combine(GlobalConfig.getDataFolder, "LastActiveProject");

        if (File.Exists(Path.Combine(handoverFile)))
            File.Delete(handoverFile);

        await File.WriteAllTextAsync(handoverFile, projectId.ToString());

        string json = await File.ReadAllTextAsync(manifestFile);

        JsonNode root = JsonNode.Parse(json)!;
        JsonObject dependencies = root["dependencies"]!.AsObject();

        dependencies[LINK_NAME] = "file:/home/matth/Documents/Projects/UnityHub/com.nexx.unityhublink";

        await File.WriteAllTextAsync(manifestFile, root.ToJsonString(new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    public async Task CreateProject(ProjectCreationInfo creation)
    {
        if (Directory.Exists(creation.info.directory))
        {
            await DependencyManager.ui!.ShowMessageBox("Project already exists", $"Failed to create project as an existing folder exists at the directory {creation.info.directory}.");
            return;
        }

        if (!await IsVersionInstalled(creation.info.version))
        {
            await DependencyManager.ui!.ShowMessageBox("Version not found", $"Failed to create the project because the unity editor version {creation.info.version} was not found.");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = await GetEditorInstall(creation.info.version!)
        };

        startInfo.ArgumentList.Add("-batchmode");
        startInfo.ArgumentList.Add("-quit");
        startInfo.ArgumentList.Add("-createProject");
        startInfo.ArgumentList.Add(creation.info.directory);

        Process process = new Process()
        {
            StartInfo = startInfo
        };

        process.Start();
        await process.WaitForExitAsync();
    }

    public struct ActiveInstances
    {
        private Process activeProcess;

        public ActiveInstances(ProcessStartInfo info)
        {
            activeProcess = new Process() { StartInfo = info };
        }

        public void Start()
        {
            activeProcess.Start();
        }
    }
}
