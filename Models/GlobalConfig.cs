using System.Runtime.InteropServices;

namespace Models;

public static class GlobalConfig
{
    public const string APPLICATION_NAME = "NexxUnityHub";
    public static string getDataFolder
    {
        get
        {
            string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APPLICATION_NAME));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }

    public static bool isOnLinux
    {
        get
        {
            if (m_isOnLinux == null)
                m_isOnLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return m_isOnLinux.Value;
        }
    }
    private static bool? m_isOnLinux;

}
