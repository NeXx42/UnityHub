using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Models;
using Models.Data;
using Models.Enums;
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

    public async Task<(EditorInfo[], int)> GetEditorDownloads(EditorFilterType filterType, string? filter, int page, int pageSize)
    {
        List<string> filters = ["order=RELEASE_DATE_DESC", $"limit={pageSize}", $"offset={page * pageSize}"];

        switch (filterType)
        {
            case EditorFilterType.LTS: filters.Add("stream=LTS"); break;
            case EditorFilterType.Alpha: filters.Add("stream=ALPHA"); break;
            case EditorFilterType.Beta: filters.Add("stream=BETA"); break;
            case EditorFilterType.Tech: filters.Add("stream=TECH"); break;

            default:
                if (!string.IsNullOrEmpty(filter))
                    filters.Add($"version={filter}");
                break;
        }

        return await GetEditorInfoFromApi(filters);
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

        await ParseEditorResponse(versionInfoJson.Values, versions);
    }

    private async Task<(EditorInfo[], int)> GetEditorInfoFromApi(params IEnumerable<string> filters)
    {
        try
        {
            using (HttpClient http = new HttpClient())
            {
                string url = $"https://services.api.unity.com/unity/editor/release/v1/releases?{string.Join("&", filters)}";
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage res = await http.SendAsync(msg);

                if (res.StatusCode == HttpStatusCode.TooManyRequests)
                    return ([], 0);

                res.EnsureSuccessStatusCode();

                string json = await res.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(json))
                    throw new Exception("Failed to get content?");

                ConcurrentDictionary<string, EditorInfo> data = new ConcurrentDictionary<string, EditorInfo>();
                int totalResults = await ParseEditorResponse([json], data);

                return (data.Values.ToArray(), totalResults);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed fetch - " + e.Message);
        }

        return ([], 0);
    }

    private async Task<int> ParseEditorResponse(IEnumerable<string> data, ConcurrentDictionary<string, EditorInfo> versions)
    {
        int totalResults = 0;

        await Parallel.ForEachAsync(data, async (string json, CancellationToken token) =>
        {
            JsonDocument doc = JsonDocument.Parse(json);
            totalResults = doc.RootElement.GetProperty("total").GetInt32();

            foreach (JsonElement result in doc.RootElement.GetProperty("results").EnumerateArray())
            {
                string version = result.GetProperty("version").GetString()!;

                versions.TryGetValue(version, out EditorInfo? info);

                if (info == null)
                {
                    info = new EditorInfo()
                    {
                        versionName = version
                    };
                    versions[version] = info;
                }

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
                {
                    try
                    {
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
                    }
                    catch { }
                }
            }
        });

        return totalResults;
    }

    private async Task GetInstalledEditorInfoForVersions(IEnumerable<EditorInstallInfo> installs)
    {
        await Parallel.ForEachAsync(installs, async (el, token) =>
        {
            string moduleInfo = Path.Combine(el.installLocation, el.versionName, "Editor", "Data", "Resources", "PackageManager", "BuiltInPackages");

            if (!Directory.Exists(moduleInfo))
                return;

            string[] packages = Directory.GetDirectories(moduleInfo);
            List<EditorInstallInfo.BuiltInPackage> packageMetadata = new(packages.Length);

            foreach (string package in packages)
            {
                string packageInfo = Path.Combine(package, "package.json");

                if (File.Exists(packageInfo))
                {
                    using (StreamReader reader = new StreamReader(packageInfo))
                    {
                        string json = await reader.ReadToEndAsync();
                        EditorInstallInfo.BuiltInPackage pkg = JsonSerializer.Deserialize<EditorInstallInfo.BuiltInPackage>(json);
                        packageMetadata.Add(pkg);
                    }
                }
            }

            el.builtInPackages = packageMetadata.ToArray();
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
            await CreateHandover(info.id);
            await InjectPackagesIntoProject(info.directory, new Dictionary<string, string>() { { LINK_NAME, "file:/home/matth/Documents/Projects/UnityHub/com.nexx.unityhublink" } });
        }

        info.lastOpened = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await database!.UpdateProjectProperties(info, [nameof(ProjectInfo.lastOpened)]);

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = await GetEditorInstall(info.version!)
        };

        startInfo.ArgumentList.Add("-projectPath");
        startInfo.ArgumentList.Add(info.directory);

        ActiveInstances instance = new ActiveInstances(info.id, startInfo, OnQuitEditor);
        activeInstances.Add(info.id, instance);

        instance.Start();
    }

    private void OnQuitEditor(int id)
    {
        activeInstances.Remove(id);
    }

    private async Task CreateHandover(int projectId)
    {
        string handoverFile = Path.Combine(GlobalConfig.getDataFolder, "LastActiveProject");

        if (File.Exists(Path.Combine(handoverFile)))
            File.Delete(handoverFile);

        await File.WriteAllTextAsync(handoverFile, projectId.ToString());
    }

    public async Task<bool> CreateProject(ProjectCreationInfo creation)
    {
        if (Directory.Exists(creation.info.directory))
        {
            await DependencyManager.ui!.ShowMessageBox("Project already exists", $"Failed to create project as an existing folder exists at the directory {creation.info.directory}.");
            return false;
        }

        if (!await IsVersionInstalled(creation.info.version))
        {
            await DependencyManager.ui!.ShowMessageBox("Version not found", $"Failed to create the project because the unity editor version {creation.info.version} was not found.");
            return false;
        }

        Exception? e = await DependencyManager.ui!.LoadProgressive("Creating", [
            new LoadRequest("Creating Project", StartProcess),
            new LoadRequest("Creating Packages", InjectPackages),
        ]);

        if (e != null)
        {
            await DependencyManager.ui!.ShowMessageBox("Error while creating project", $"Failed to create project due to the following error\n{e.Message}.");
            return false;
        }

        return true;

        async Task StartProcess(CancellationToken token)
        {
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

        async Task InjectPackages(CancellationToken token)
        {
            await InjectPackagesIntoProject(creation.info.directory, creation.packages);
        }
    }

    public async Task InstallEditor(EditorInfo version, int download, string path)
    {
        if (await IsVersionInstalled(version.versionName))
        {
            await DependencyManager.ui!.ShowMessageBox("Install already exists", $"Failed to install version {version.versionName} because it is already installed.");
            return;
        }

        string savePath = Path.Combine(path, version.versionName);
        string extractPath = $"{savePath}.{version.downloads[download].type!.ToLower()}";

        using (HttpClient http = new HttpClient())
        {
            HttpResponseMessage res = await http.GetAsync(version.downloads[download].url, HttpCompletionOption.ResponseHeadersRead);

            res.EnsureSuccessStatusCode();

            await using Stream stream = await res.Content.ReadAsStreamAsync();
            await using FileStream file = File.Create(extractPath);

            await stream.CopyToAsync(file);
            await Extract(extractPath, savePath);
            await Extract(savePath, savePath);

            File.Delete(Path.Combine(savePath, version.versionName));
            File.Delete(extractPath);
        }
    }

    private async Task Extract(string path, string result)
    {
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "7z";

        info.RedirectStandardError = true;
        info.RedirectStandardOutput = true;
        info.UseShellExecute = false;
        info.CreateNoWindow = true;

        info.ArgumentList.Add("x");
        info.ArgumentList.Add(path);

        info.ArgumentList.Add($"-o{result}");

        info.ArgumentList.Add("-y");
        info.ArgumentList.Add("-bsp1");

        Process p = new Process();
        p.StartInfo = info;

        p.Start();
        await ReadProgressOfExtraction(p, CancellationToken.None);

        if (p.ExitCode != 0)
        {
            throw new Exception(await p.StandardError.ReadToEndAsync());
        }
    }

    private static Task ReadProgressOfExtraction(Process p, CancellationToken cancellationToken)
    {
        int charNumber;
        const int newLineCharNumber = '\b';

        string line = string.Empty;

        TaskCompletionSource task = new TaskCompletionSource();

        Task.Run(() =>
        {
            while (!p.HasExited)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    p.Kill();
                    return;
                }

                while ((charNumber = p.StandardOutput.Read()) != -1)
                {
                    if (charNumber == newLineCharNumber)
                    {
                        string percentageText = line.Replace(" ", "");
                        Match match = Regex.Match(percentageText, @"^(\d+)%");

                        if (match.Success)
                        {
                            int percentage = int.Parse(match.Groups[1].Value);
                            //progress.Report(percentage);
                        }

                        line = string.Empty;
                    }
                    else
                    {
                        line += (char)charNumber;
                    }
                }
            }

            task.SetResult();
        });

        return task.Task;
    }

    private async Task InjectPackagesIntoProject(string projectRoot, Dictionary<string, string> packages)
    {
        string manifestFile = Path.Combine(projectRoot, "Packages", "manifest.json");

        if (!File.Exists(manifestFile))
        {
            Console.WriteLine("Failed to find manifest file? invalid project");
            return;
        }

        string json = await File.ReadAllTextAsync(manifestFile);

        JsonNode root = JsonNode.Parse(json)!;
        JsonObject dependencies = root["dependencies"]!.AsObject();

        foreach (KeyValuePair<string, string> pkg in packages)
            dependencies[pkg.Key] = pkg.Value;

        await File.WriteAllTextAsync(manifestFile, root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    public struct ActiveInstances
    {
        private int id;
        private Process activeProcess;

        public ActiveInstances(int id, ProcessStartInfo info, Action<int> onExit)
        {
            this.id = id;

            activeProcess = new Process() { StartInfo = info };
            activeProcess.Exited += (_, __) => onExit(id);
        }

        public void Start()
        {
            activeProcess.Start();
        }
    }
}
