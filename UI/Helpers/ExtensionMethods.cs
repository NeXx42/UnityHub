using System;
using System.Threading;
using System.Threading.Tasks;

namespace UI.Helpers;

public static class ExtensionMethods
{
    public static TaskCompletionSource WithCancellationToken(this TaskCompletionSource task, CancellationToken token)
    {
        token.Register(task.SetCanceled);
        return task;
    }


    public static TaskCompletionSource<T> WithCancellationToken<T>(this TaskCompletionSource<T> task, CancellationToken token)
    {
        token.Register(task.SetCanceled);
        return task;
    }
}
