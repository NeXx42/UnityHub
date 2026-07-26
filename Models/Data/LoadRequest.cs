namespace Models.Data;

public struct LoadRequest
{
    public string msg;
    private bool runInBackground;

    public Func<CancellationToken, Task>? task;
    public Func<IProgress<float>, CancellationToken, Task>? taskWithProgress;

    public LoadRequest(string msg, Func<CancellationToken, Task> task, bool runInBackground = false)
    {
        this.msg = msg;
        this.task = task;
        this.taskWithProgress = null;
        this.runInBackground = runInBackground;
    }

    public LoadRequest(string msg, Func<IProgress<float>, CancellationToken, Task> task, bool runInBackground = false)
    {
        this.msg = msg;
        this.task = null;
        this.taskWithProgress = task;
        this.runInBackground = runInBackground;
    }

    public async Task<Exception?> Run(CancellationToken token, IProgress<float>? secondaryProgress = null)
    {
        try
        {
            Task operation = RunInternal(token, secondaryProgress);

            if (runInBackground)
            {
                await Task.Run(() => operation, token);
            }
            else
            {
                await operation;
            }
        }
        catch (Exception e)
        {
            return e;
        }

        return null;
    }

    private async Task RunInternal(CancellationToken token, IProgress<float>? secondaryProgress = null)
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
}
