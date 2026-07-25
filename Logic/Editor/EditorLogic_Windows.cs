using System.Diagnostics;
using Models.Data;

namespace Logic.Editor;

public class EditorLogic_Windows : EditorLogic
{
    protected override bool IsEditorDownloadSupported(string platform, string architecture)
    {
        return platform.Equals("WINDOWS") && architecture.Equals("X86_64");
    }

    protected override LoadRequest[] DownloadEditorInternal(EditorInfo version, string path)
    {
        string editorRoot = Path.Combine(path, version.versionName);
        string tempExeDir = Path.Combine(editorRoot, "_temp", "setup.exe");

        return [
            new LoadRequest("Donwload", DonwloadInstaller),
            new LoadRequest("Install", Install),
        ];

        async Task DonwloadInstaller(IProgress<float> p, CancellationToken c)
            => await EditorInstallHelper.DownloadFile(version.download!.Value.url!, tempExeDir, p, c);

        async Task Install(IProgress<float> p, CancellationToken c)
        {
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = tempExeDir,
                UseShellExecute = true,
                Verb = "runas"
            };
            info.ArgumentList.Add("/S");
            info.ArgumentList.Add($"/D={editorRoot}");

            Process installProcess = new Process()
            {
                StartInfo = info
            };

            installProcess.Start();
            await installProcess.WaitForExitAsync(c);

            if (installProcess.ExitCode != 0)
                throw new InvalidOperationException(
                    $"Unity installation failed with exit code {installProcess.ExitCode}.");

            p?.Report(1f);
        }
    }

    protected override string GetEditorInstallBinary(string rootName) => Path.Combine(rootName, "Editor", "Unity.exe");
}
