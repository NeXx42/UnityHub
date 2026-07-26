namespace Models.Helpers;

public static class LoggingHelper
{
    private static string getLogFilePath => Path.Combine(GlobalConfig.getDataFolder, "log.txt");

    public static void ClearLog()
    {
        try
        {
            if (File.Exists(getLogFilePath))
                File.Delete(getLogFilePath);
        }
        catch
        {
            Console.WriteLine("Failed to delete logfile");
        }
    }

    public static void LogError(string error)
    {
        string msg = $"[ERROR {DateTime.UtcNow}] {error}";

        Console.WriteLine(msg);
        WriteToLog([msg]);
    }

    public static void LogError(Exception e)
    {
        string[] lines = [$"[ERROR {DateTime.UtcNow}] {e.Message}", .. (e.StackTrace ?? "").Split("\n")];

        foreach (string line in lines)
            Console.WriteLine(line);

        WriteToLog(lines);
    }

    public static void LogWarning(string msg)
    {
        msg = $"[WARNING {DateTime.UtcNow}] {msg}";

        Console.WriteLine(msg);
        WriteToLog([msg]);
    }

    public static void Log(string msg)
    {
        msg = $"[LOG {DateTime.UtcNow}] {msg}";

        Console.WriteLine(msg);
        WriteToLog([msg]);
    }

    private static void WriteToLog(params string[] lines)
    {
        try
        {
            File.AppendAllLines(getLogFilePath, lines);
        }
        catch { Console.WriteLine("Failed to write to log"); }
    }
}
