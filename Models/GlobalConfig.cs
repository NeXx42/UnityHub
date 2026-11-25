using System.Runtime.InteropServices;

namespace Models;

public static class GlobalConfig
{
    public const string APPLICATION_NAME = "NexxUnityHub";
    public static string getDataFolder
    {
        get
        {
            string path;

            if (isOnFlatpak)
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", APPLICATION_NAME);
            else
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APPLICATION_NAME);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }

    public static char GetLineEnding() => isOnLinux ? '/' : '\\';

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

    public static bool isOnFlatpak
    {
        get
        {
            if (m_isOnFlatpak.HasValue)
                return m_isOnFlatpak.Value;

            return (m_isOnFlatpak = isOnLinux && File.Exists("/.flatpak-info")).Value;
        }
    }
    private static bool? m_isOnFlatpak;
}
