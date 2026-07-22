using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
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

public abstract class EditorLogic : IEditorLogic
{
    public IDataRepository database => DependencyManager.GetService<IDataRepository>()!;
    public const string LINK_NAME = "com.nexx.unityhublink";

    private string[]? editorLocations;
    private Dictionary<int, ActiveInstances> activeInstances = new();
    private Dictionary<string, ActiveDownload> activeDownloads = new();

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

                ParseEditorResponse(info, result);
            }
        });

        return totalResults;
    }

    protected virtual void ParseEditorResponse(EditorInfo info, JsonElement result)
    {
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
            foreach (JsonElement download in downloads.EnumerateArray())
            {
                if (download.TryGetProperty("platform", out JsonElement plat) && download.TryGetProperty("architecture", out JsonElement arch))
                {
                    if (!IsEditorDownloadSupported(plat.GetString() ?? "", arch.GetString() ?? ""))
                        continue;

                    info.download = new EditorInfo.Download()
                    {
                        url = TryParse<string>(download, "url"),
                        type = TryParse<string>(download, "type"),
                        platform = TryParse<string>(download, "platform"),
                        architecture = TryParse<string>(download, "architecture"),

                        downloadSize = download.TryGetProperty("downloadSize", out JsonElement downloadSize) ? TryParse<ulong>(downloadSize, "value") : 0,
                        installSize = download.TryGetProperty("installSize", out JsonElement installSize) ? TryParse<ulong>(installSize, "value") : 0,

                        integrity = TryParse<string>(download, "integrity"),

                        modules = download.GetProperty("modules").EnumerateArray().Select(a => new EditorInfo.Download.Module()
                        {
                            id = TryParse<string>(a, nameof(EditorInfo.Download.Module.id)),
                            slug = TryParse<string>(a, nameof(EditorInfo.Download.Module.slug)),
                            description = TryParse<string>(a, nameof(EditorInfo.Download.Module.description)),
                            name = TryParse<string>(a, nameof(EditorInfo.Download.Module.name)),
                            url = TryParse<string>(a, nameof(EditorInfo.Download.Module.url)),
                            type = TryParse<string>(a, nameof(EditorInfo.Download.Module.type)),

                            //downloadSize = a.TryGetProperty(nameof(EditorInfo.Download.Module.downloadSize), out JsonElement moduleDownloadSize) ? TryParse<ulong>(moduleDownloadSize, "value") : 0,
                            //installedSize = a.TryGetProperty(nameof(EditorInfo.Download.Module.installedSize), out JsonElement moduleInstalledSize) ? TryParse<ulong>(moduleInstalledSize, "value") : 0,

                            required = TryParse<bool>(a, nameof(EditorInfo.Download.Module.required)),
                            hidden = TryParse<bool>(a, nameof(EditorInfo.Download.Module.hidden)),
                            preSelected = TryParse<bool>(a, nameof(EditorInfo.Download.Module.preSelected)),

                            integrity = TryParse<string>(a, nameof(EditorInfo.Download.Module.integrity)),
                            destination = TryParse<string>(a, nameof(EditorInfo.Download.Module.destination)),

                        }).ToArray()
                    };

                    break;
                }
            }
        }

        T? TryParse<T>(JsonElement parent, string key)
        {
            if (parent.TryGetProperty(key, out JsonElement el))
            {
                try
                {
                    return el.Deserialize<T>();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return default;
        }
    }

    protected abstract bool IsEditorDownloadSupported(string platform, string architecture);

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
            await CreateInjectorPackage(info);

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

    public async Task InstallEditor(EditorInfo version, string path)
    {
        if (activeDownloads.ContainsKey(version.versionName))
        {
            await DependencyManager.ui!.ShowMessageBox("Already downloading", $"Failed to install version {version.versionName} because it is already being downloaded.");
            return;
        }

        if (await IsVersionInstalled(version.versionName))
        {
            await DependencyManager.ui!.ShowMessageBox("Install already exists", $"Failed to install version {version.versionName} because it is already installed.");
            return;
        }

        if (!version.download.HasValue)
        {
            await DependencyManager.ui!.ShowMessageBox("No available download", $"Failed to install version {version.versionName} because it there is no download avaliable.");
            return;
        }

        activeDownloads[version.versionName] = new ActiveDownload(version, DownloadEditorInternal(version, path));
    }

    protected abstract LoadRequest[] DownloadEditorInternal(EditorInfo download, string path);

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

    private async Task CreateInjectorPackage(ProjectInfo info)
    {
        // need some way of determining the version to replace it on updates
        const string packageName = "com.nexx.unityhublink";
        string packageLocation = Path.Combine(GlobalConfig.getDataFolder, packageName);

        if (!Directory.Exists(packageLocation))
        {
            string referencePackage = Path.Combine(AppContext.BaseDirectory, packageName);

            if (!Directory.Exists(referencePackage))
            {
                Console.WriteLine("Failed to find injector package, skipping");
                return;
            }

            CopyFromReference(referencePackage, packageLocation);

            void CopyFromReference(string existing, string destination)
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
                    CopyFromReference(directory, destSubDir);
                }
            }
        }

        await InjectPackagesIntoProject(info.directory, new Dictionary<string, string>() { { LINK_NAME, $"file:{packageLocation}" } });
    }

    public Dictionary<EditorInfo, DownloadStatus> GetActiveInstalls()
    {
        KeyValuePair<string, ActiveDownload>[] inDownloads = activeDownloads.ToArray();
        Dictionary<EditorInfo, DownloadStatus> res = new(inDownloads.Length);

        foreach (KeyValuePair<string, ActiveDownload> download in inDownloads)
        {
            if (download.Value.isDone)
            {
                activeDownloads.Remove(download.Key);
                continue;
            }

            res.Add(download.Value.editorInfo, download.Value);
        }

        return res;
    }

    public void StopActiveInstall(string version)
    {
        if (activeDownloads.TryGetValue(version, out ActiveDownload? download) && download != null)
        {
            download.Stop();
            activeDownloads.Remove(version);
        }
    }

    public async Task Delete(string version)
    {
        string path = (await GetEditorInstall(version))!;
        string dir = Directory.GetParent(path)!.Parent!.FullName;

        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
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

    private class ActiveDownload : DownloadStatus
    {
        public EditorInfo editorInfo { private set; get; }

        private Thread thread;
        private CancellationTokenSource cancellation;

        private IProgress<float> mainProgress;
        private IProgress<float> secondaryProgress;
        private LoadRequest[] loadRequests;

        public float mainProgressValue { private set; get; }
        public float secondaryProgressValue { private set; get; }

        public ActiveDownload(EditorInfo editorInfo, params LoadRequest[] loads)
        {
            this.editorInfo = editorInfo;

            loadRequests = loads;
            cancellation = new CancellationTokenSource();

            mainProgress = new Progress<float>(v => mainProgressValue = v);
            secondaryProgress = new Progress<float>(v =>
            {
                secondaryProgressValue = v;
                currentValue = v;
            });

            thread = new Thread(Run);
            thread.Start();

            isDone = false;
        }

        private void Run()
        {
            float interval = 1 / loadRequests.Length;
            float curProgress = 0;

            foreach (LoadRequest req in loadRequests)
            {
                if (cancellation.IsCancellationRequested)
                    break;

                req.Run(cancellation.Token, secondaryProgress).Wait();
                mainProgress.Report(curProgress += interval);
            }

            isDone = true;
        }

        public void Stop()
        {
            cancellation.Cancel();
        }
    }
}
