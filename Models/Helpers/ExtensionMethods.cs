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

    public static async Task WhenAllProgressive(this LoadRequest[] tasks, CancellationToken token)
    {
        foreach (LoadRequest req in tasks)
            await req.Run(token);
    }
}
