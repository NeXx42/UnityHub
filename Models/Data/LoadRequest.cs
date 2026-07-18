namespace Models.Data;

public struct LoadRequest
{
    public string msg;
    public Func<CancellationToken, Task> task;

    public LoadRequest(string msg, Func<CancellationToken, Task> task)
    {
        this.msg = msg;
        this.task = task;
    }

    public async Task Run(CancellationToken token)
    {
        await task(token);
    }
}
