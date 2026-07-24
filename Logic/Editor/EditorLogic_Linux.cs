using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Models.Data;

namespace Logic.Editor;

public class EditorLogic_Linux : EditorLogic
{
    protected override bool IsEditorDownloadSupported(string platform, string architecture)
    {
        return platform.Equals("LINUX") && architecture.Equals("X86_64");
    }

    protected override LoadRequest[] DownloadEditorInternal(EditorInfo version, string path)
    {
        string editorRoot = Path.Combine(path, version.versionName);
        string downloadPath = Path.Combine(editorRoot, "_temp", "editor.crdownload");
        string intermediateStep = Path.Combine(editorRoot, "_temp", "editor.tar");

        return [
            new LoadRequest("Download", DownloadFile),
            new LoadRequest("Unzip", Unzip1),
            new LoadRequest("Unzip", Unzip2),
        ];

        async Task DownloadFile(IProgress<float> subProgress, CancellationToken token)
            => await EditorInstallHelper.DownloadFile(version.download!.Value.url!, downloadPath, subProgress, token);

        async Task Unzip1(IProgress<float> subProgress, CancellationToken token)
            => await EditorInstallHelper.Extract(downloadPath, intermediateStep, token, subProgress);

        async Task Unzip2(IProgress<float> subProgress, CancellationToken token)
            => await EditorInstallHelper.Extract(intermediateStep, editorRoot, token, subProgress);
    }
}
