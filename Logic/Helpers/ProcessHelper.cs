using System.Diagnostics;
using System.Reflection.Metadata;
using Models;

namespace Logic.Helpers;

public static class ProcessHelper
{
    public static Process Run(ProcessStartInfo info)
    {
        return new Process()
        {
            StartInfo = info.UpdateProcessStartInfoForEnv()
        };
    }

    public static void BrowseToLocation(params string[] path)
    {
        string dir = Path.Combine(path);

        if (!Directory.Exists(dir))
            throw new Exception($"{dir} was not found");

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = GlobalConfig.isOnLinux ? "xdg-open" : "explorer.exe",
            UseShellExecute = GlobalConfig.isOnLinux ? false : true,
        };

        startInfo.ArgumentList.Add(dir);
        Run(startInfo).Start();
    }

    public static ProcessStartInfo UpdateProcessStartInfoForEnv(this ProcessStartInfo info)
    {
        if (GlobalConfig.isOnFlatpak)
        {
            string[] args = [info.FileName, .. info.ArgumentList];

            info.FileName = "flatpak-spawn";

            info.ArgumentList.Clear();
            info.ArgumentList.Add("--host");

            foreach (string arg in args)
                info.ArgumentList.Add(arg);
        }

        return info;
    }
}
