using System.Diagnostics;
using Models.Data;

namespace Logic;

public static class EditorLogic
{
    public static string[] editorLocations = [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Unity", "Hub", "Editor")
    ];

    public static bool IsVersionInstalled(string? version) => !string.IsNullOrEmpty(GetEditorInstall(version));

    public static string? GetEditorInstall(string? version)
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

    public static async Task LaunchProject(int id) => await LaunchProject(await ProjectLogic.GetProjectInfo(id));
    public static async Task LaunchProject(ProjectInfo info)
    {
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
}
