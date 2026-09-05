using System.Diagnostics;
using System.Reflection.Metadata;

namespace Logic.Helpers;

public static class ProcessHelper
{
    public static void Run(string command, params string[] args)
    {

    }

    public static Process Run(ProcessStartInfo info)
    {
        return new Process()
        {
            StartInfo = info.UpdateProcessStartInfoForEnv()
        };
    }

    public static void Run(Process info)
    {

    }

    public static ProcessStartInfo UpdateProcessStartInfoForEnv(this ProcessStartInfo info)
    {
        if (File.Exists("/.flatpak-info"))
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
