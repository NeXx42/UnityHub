namespace Models.Helpers;

public static class ExtensionMethods
{
    public static string GetDisplayName<T>(this T value) where T : Enum
    {
        string str = value.ToString();
        string[] words = str.Split("_");

        return string.Join(" ", words);
    }
}
