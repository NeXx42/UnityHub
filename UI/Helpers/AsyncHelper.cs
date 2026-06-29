using System;
using System.Threading.Tasks;

namespace UI.Helpers;

public static class AsyncHelper
{
    public static void WrapTask(Func<Task> task)
    {
        try
        {
            _ = task();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to execute\n{e.Message}");
        }
    }

    public static void WrapTask<T>(T inp, Func<T, Task> task)
    {
        try
        {
            _ = task(inp);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to execute\n{e.Message}");
        }
    }
}
