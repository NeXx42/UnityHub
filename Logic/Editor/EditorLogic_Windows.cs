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
        throw new NotImplementedException();
    }
}
