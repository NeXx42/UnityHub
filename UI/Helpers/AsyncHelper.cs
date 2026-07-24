using System;
using System.Threading.Tasks;
using Logic;

namespace UI.Helpers;

public static class AsyncHelper
{
    public static void Wrap(this Task task)
    {
        HandleException(() => _ = task);
    }

    public static void Wrap(this Func<Task>? task)
    {
        if (task == null)
            return;

        HandleException(() => _ = task());
    }

    public static void WrapTask(Func<Task>? task)
    {
        if (task == null)
            return;

        HandleException(() => _ = task());
    }

    public static void WrapTask<T>(T inp, Func<T, Task>? task)
    {
        if (task == null)
            return;

        HandleException(() => _ = task(inp));
    }

    public static void WrapTask<T>(this Func<T, Task>? task, T inp)
    {
        if (task == null)
            return;

        HandleException(() => _ = task(inp));
    }

    private static void HandleException(Action callback)
    {
        try
        {
            callback();
        }
        catch (Exception e)
        {
            DependencyManager.ui?.ShowMessageBox(e);
            Console.WriteLine($"Failed to execute\n{e.Message}");
        }
    }
}
