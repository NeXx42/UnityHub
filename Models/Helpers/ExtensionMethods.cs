using Models.Data;

namespace Models.Helpers;

public static class ExtensionMethods
{
    public static string GetDisplayName<T>(this T value) where T : Enum
    {
        string str = value.ToString();
        string[] words = str.Split("_");

        return string.Join(" ", words);
    }

    public static async Task<Exception?> WhenAllProgressive(this LoadRequest[] tasks, CancellationToken token)
    {
        Exception? e;

        foreach (LoadRequest req in tasks)
        {
            e = await req.Run(token);

            if (e == null)
                return e;
        }

        return null;
    }

    public static string FormatSize(this long? size)
        => size.HasValue ? FormatSize((ulong)size) : "Unknown";

    public static string FormatSize(this ulong? size)
    {
        if (!size.HasValue)
            return "Unknown";

        string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        double curSize = size.Value;
        int unit = 0;

        while (curSize >= 1024 && unit < units.Length - 1)
        {
            curSize /= 1024;
            unit++;
        }

        return $"{curSize:0.#} {units[unit]}";
    }

}
