namespace Models.Data;

public struct LoadRequest
{
    public string msg;

    public Func<CancellationToken, Task>? task;
    public Func<IProgress<float>, CancellationToken, Task>? taskWithProgress;

    public LoadRequest(string msg, Func<CancellationToken, Task> task)
    {
        this.msg = msg;
        this.task = task;
        this.taskWithProgress = null;
    }

    public LoadRequest(string msg, Func<IProgress<float>, CancellationToken, Task> task)
    {
        this.msg = msg;
        this.task = null;
        this.taskWithProgress = task;
    }

    public async Task<Exception?> Run(CancellationToken token, IProgress<float>? secondaryProgress = null)
    {
        try
        {
            if (taskWithProgress != null)
            {
                secondaryProgress ??= new Progress<float>();
                secondaryProgress.Report(0);

                await taskWithProgress(secondaryProgress, token);
                secondaryProgress.Report(1);
            }
            else
            {
                await task!(token);
            }
        }
        catch (Exception e)
        {
            return e;
        }

        return null;
    }
}
