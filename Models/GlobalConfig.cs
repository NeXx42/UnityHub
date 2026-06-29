using System.Runtime.InteropServices;

namespace Models;

public class GlobalConfig
{
    public const string APPLICATION_NAME = "NexxUnityHub";

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
