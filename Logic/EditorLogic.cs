using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Models;
using Models.Data;
using Models.Interfaces;

namespace Logic;

public class EditorLogic : IEditorLogic
{
    public const string LINK_NAME = "com.nexx.unityhublink";

    public static string[] editorLocations = [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Unity", "Hub", "Editor")
    ];

    public bool IsVersionInstalled(string? version) => !string.IsNullOrEmpty(GetEditorInstall(version));

    public string? GetEditorInstall(string? version)
    {
        if (string.IsNullOrEmpty(version))
            return null;

        string path;

        foreach (string root in editorLocations)
        {
            path = Path.Combine(root, version);

            if (Directory.Exists(path))
                return Path.Combine(path, "Editor", "Unity");
        }

        return null;
    }

    public async Task LaunchProject(int id) => await LaunchProject(await DependencyManager.GetService<IProjectLogic>()!.GetProjectInfo(id));
    public async Task LaunchProject(ProjectInfo? info)
    {
        if (info == null)
        {
            await DependencyManager.ui!.ShowMessageBox("Project is invalid", "Failed to launch the project because the id didn't correspond with a known entry.");
            return;
        }

        if (!IsVersionInstalled(info.version))
        {
            await DependencyManager.ui!.ShowMessageBox("Version not found", $"Failed to launch the project because the unity editor version {info.version} was not found.");
            return;
        }

        if (true)
        {
            await InjectLinker(info.directory, info.id);
        }

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = GetEditorInstall(info.version!)
        };

        startInfo.ArgumentList.Add("-projectPath");
        startInfo.ArgumentList.Add(info.directory);

        Process process = new Process()
        {
            StartInfo = startInfo
        };

        process.Start();
    }

    public async Task DeriveProjectInfo(ProjectInfo info)
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
}
