using System.Diagnostics;
using Models.Data;
using Models.Enums;
using Models.Interfaces;

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

            if (await DependencyManager.GetService<IConfigLogic>()!.Get(ConfigEntry.Windows_InstallSilent, Config_EnabledStatus.Enabled) == Config_EnabledStatus.Enabled)
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

    protected override async Task DownloadEditorModuleInternal(EditorInfo.Download.Module module, string destination, string tempDir, IProgress<float> progress, CancellationToken token)
    {
        if (module.type?.ToLower().Equals("exe") ?? false)
        {
            await InstallProcess();
            return;
        }

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

        async Task InstallProcess()
        {
            string filename = $"{Guid.NewGuid()}.exe";
            await EditorInstallHelper.DownloadFile(module.url!, Path.Combine(tempDir, filename), progress, token);

            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = filename,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process p = new Process()
            {
                StartInfo = info
            };
            p.Start();

            await p.WaitForExitAsync();
            return;
        }
    }
}
