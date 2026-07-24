using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logic.Editor;
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

    private Action<float?>? callbackGlobalDownloadProgress;

    public void RegisterGlobalInstallProgressUpdate(Action<float?> callback) => callbackGlobalDownloadProgress += callback;

    private void RecalculateGlobalInstallProgress()
    {
        float? total = null;
        int entries = 0;

        foreach (var download in activeDownloads)
        {
            if (download.Value.isDone)
                continue;

            entries++;

            total ??= 0;
            total += download.Value.currentValue;
        }

        if (entries == 0)
            callbackGlobalDownloadProgress?.Invoke(null);
        else
            callbackGlobalDownloadProgress?.Invoke(total / entries);
    }

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
                if (!installs.Contains(version) && !activeDownloads.ContainsKey(version))
                    installs.Add(version);
        }

        return installs.ToArray();
    }

    public async Task<EditorInstallInfo[]> GetEditorMetadataForDownloadedVersions(CancellationToken token)
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

    public async Task<EditorInfo?> GetEditorMetadata(string versionName)
    {
        (EditorInfo[] info, _) = await GetEditorInfoFromApi($"version={versionName}");
        return info.FirstOrDefault();
    }

    public async Task<(EditorInfo[], int)> SearchEditorDownloads(EditorFilterType filterType, string? filter, int page, int pageSize)
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

    /// <summary>
    /// this is bad, i need to make it more generic such that i can store the response per version in the db
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
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
                            id = a.GetProperty(nameof(EditorInfo.Download.Module.id)).GetString()!,
                            slug = TryParse<string>(a, nameof(EditorInfo.Download.Module.slug)),
                            description = TryParse<string>(a, nameof(EditorInfo.Download.Module.description)),
                            category = TryParse<string>(a, nameof(EditorInfo.Download.Module.category)),
                            name = TryParse<string>(a, nameof(EditorInfo.Download.Module.name)),
                            url = TryParse<string>(a, nameof(EditorInfo.Download.Module.url)),
                            type = TryParse<string>(a, nameof(EditorInfo.Download.Module.type)),

                            downloadSize = TryParseSize(a, nameof(EditorInfo.Download.Module.downloadSize)),
                            installedSize = TryParseSize(a, nameof(EditorInfo.Download.Module.installedSize)),

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

        ulong TryParseSize(JsonElement parent, string key)
        {
            if (!parent.TryGetProperty(key, out JsonElement child))
                return 0;

            if (child.TryGetProperty("value", out JsonElement el))
            {
                el.TryGetDecimal(out decimal d);
                return (ulong)Math.Round(d);
            }

            return 0;
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
            el.installedPackages = new();

            if (el.download.HasValue)
            {
                foreach (EditorInfo.Download.Module module in el.download.Value.modules)
                {
                    string? moduleExtractPath = module.destination?.Replace("{UNITY_PATH}", Path.Combine(el.installLocation, el.versionName));

                    if (string.IsNullOrEmpty(moduleExtractPath) || !DoesHaveModuleInstalled(module, moduleExtractPath))
                        continue;

                    el.installedPackages.Add(module.id);
                }
            }

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

        bool DoesHaveModuleInstalled(EditorInfo.Download.Module module, string moduleRoot)
        {
            switch (module.category ?? "")
            {
                case "":
                    return false;

                case "LANGUAGE_PACK":
                case "Language packs":
                case "Language packs (Preview)":
                    string? languagePackName = module.id?.Replace("language-", "");
                    return File.Exists(Path.Combine(moduleRoot, $"{languagePackName}.po"));

                default:
                    return Directory.Exists(moduleRoot);
            }
        }
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
            if (await DependencyManager.ui!.ShowConfirmationBox("Version not found", $"Failed to launch the project because the unity editor version {info.version} was not found.\nYou can update the version or install the edtior.",
                new ConfirmationButton("Cancel"), new ConfirmationButton("Install", true)) != 1)
                return;

            await DependencyManager.ui.RequestVersionInstall(info.version);
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

    public async Task InstallEditor(EditorInfo version, HashSet<string> desiredModules, string? path)
    {
        if (activeDownloads.ContainsKey(version.versionName))
        {
            await DependencyManager.ui!.ShowMessageBox("Already downloading", $"Failed to install version {version.versionName} because it is already being downloaded.");
            return;
        }

        if (!version.download.HasValue)
        {
            await DependencyManager.ui!.ShowMessageBox("No available download", $"Failed to install version {version.versionName} because it there is no download avaliable.");
            return;
        }

        LoadRequest[] toInstall;
        string editorVersionRoot;

        if (version is EditorInstallInfo installedVersion)
        {
            editorVersionRoot = Path.Combine(installedVersion.installLocation, version.versionName);
            await EnsureManifestExists(editorVersionRoot);

            toInstall = [];
        }
        else
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                await DependencyManager.ui!.ShowMessageBox("Invalid Path", $"Failed to install version {version.versionName} because the provided path ({path}) is invalid.");
                return;
            }

            if (await IsVersionInstalled(version.versionName))
            {
                await DependencyManager.ui!.ShowMessageBox("Install already exists", $"Failed to install version {version.versionName} because it is already installed.");
                return;
            }

            editorVersionRoot = Path.Combine(path!, version.versionName);

            Directory.CreateDirectory(editorVersionRoot);
            await EnsureManifestExists(editorVersionRoot);

            toInstall = DownloadEditorInternal(version, path);
        }

        EditorInfo.Download.Module[] modulesToInstall = version.download.Value.modules
            .Where(m => desiredModules.Contains(m.id))
            .ToArray();

        activeDownloads[version.versionName] = new ActiveDownload(version, editorVersionRoot, RecalculateGlobalInstallProgress, [.. toInstall, .. InstallEditorSubModules(editorVersionRoot, modulesToInstall)]);

        async Task EnsureManifestExists(string editorVersionRoot)
        {
            if (File.Exists(Path.Combine(editorVersionRoot, "modules.json")))
                return;

            await File.WriteAllTextAsync(Path.Combine(editorVersionRoot, "modules.json"), JsonSerializer.Serialize(version.download.Value.modules, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }

    private LoadRequest[] InstallEditorSubModules(string editorRoot, params EditorInfo.Download.Module[] modules)
    {
        string tempDir = Path.Combine(editorRoot, "_temp");

        return modules.Select(m => new LoadRequest($"Installing {m.name}", (p, c) => Install(m, p, c))).ToArray();

        async Task Install(EditorInfo.Download.Module module, IProgress<float> progress, CancellationToken token)
        {
            string destination = module.destination!.Replace("{UNITY_PATH}", editorRoot);

            switch (module.category)
            {
                case "LANGUAGE_PACK":
                case "Language packs":
                case "Language packs (Preview)":
                    await InstallLanguagePack(module, progress, token);
                    break;
            }

            async Task InstallLanguagePack(EditorInfo.Download.Module module, IProgress<float> progress, CancellationToken token)
            {
                string languagePackName = $"{module.id!.Replace("language-", "")}.po";
                await EditorInstallHelper.DownloadFile(module.url!, Path.Combine(tempDir, languagePackName), progress, token);

                Directory.CreateDirectory(destination);
                File.Move(Path.Combine(tempDir, languagePackName), Path.Combine(destination, languagePackName));
            }
        }
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

    public void BrowseToEditor(EditorInfo? info)
    {
        if (info == null)
            return;

        if (info is EditorInstallInfo installInfo)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "xdg-open",
                UseShellExecute = false,
            };

            startInfo.ArgumentList.Add(Path.Combine(installInfo.installLocation, installInfo.versionName));

            Process process = new Process()
            {
                StartInfo = startInfo
            };

            process.Start();
        }
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

        private string root;

        public float mainProgressValue { private set; get; }
        public float secondaryProgressValue { private set; get; }

        public ActiveDownload(EditorInfo editorInfo, string root, Action updateGlobalProgress, params LoadRequest[] loads)
        {
            this.root = root;
            this.editorInfo = editorInfo;

            loadRequests = loads;
            cancellation = new CancellationTokenSource();

            mainProgress = new Progress<float>(v =>
            {
                mainProgressValue = v;
                secondaryProgressValue = 0;

                currentValue = mainProgressValue * (1 / (float)loadRequests.Length);
                updateGlobalProgress();
            });

            secondaryProgress = new Progress<float>(v =>
            {
                secondaryProgressValue = v;

                float interval = 1 / (float)loadRequests.Length;
                currentValue = (mainProgressValue * interval) + (secondaryProgressValue * interval);
                updateGlobalProgress();
            });

            thread = new Thread(() => _ = Run());
            thread.Start();

            isDone = false;
        }

        private async Task Run()
        {
            string tempDir = Path.Combine(root, "_temp");

            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            Directory.CreateDirectory(tempDir);

            try
            {
                for (int i = 0; i < loadRequests.Length; i++)
                {
                    if (cancellation.IsCancellationRequested)
                        break;

                    mainProgress.Report(i);
                    Exception? e = await loadRequests[i].Run(cancellation.Token, secondaryProgress);

                    if (e != null)
                    {
                        error = e;
                        break;
                    }
                }

                isDone = true;
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        public void Stop()
        {
            cancellation.Cancel();
        }
    }
}
