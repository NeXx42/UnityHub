using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace UI.Helpers;

public static class IconFetcher
{
    public delegate void FetchImageEvent(string? id, Bitmap? img);

    private static ConcurrentDictionary<string, ImageFetchRequest> queuedImageFetch = new ConcurrentDictionary<string, ImageFetchRequest>();
    private static ConcurrentDictionary<string, Bitmap?> cachedImages = new ConcurrentDictionary<string, Bitmap?>();

    private static Thread? imageFetchThread;

    public static async Task<Bitmap?> GetImage(string? path, CancellationToken? token = null)
    {
        TaskCompletionSource<Bitmap?> task = new TaskCompletionSource<Bitmap?>(token ?? CancellationToken.None);
        await GetImage(path, (_, a) => task.TrySetResult(a));

        return await task.Task;
    }

    public static async Task GetImage(string? path, FetchImageEvent onFetch)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            onFetch?.Invoke(path, null);
            return;
        }

        if (cachedImages.TryGetValue(path, out Bitmap? res))
        {
            onFetch?.Invoke(path, res);
            return;
        }

        if (imageFetchThread == null)
        {
            imageFetchThread = new Thread(GameImageFetcher);
            imageFetchThread.Name = "Image Thread";
            imageFetchThread.Start();
        }

        queuedImageFetch.AddOrUpdate(path, new ImageFetchRequest()
        {
            path = path,
            callback = MediateReturn
        },
        (_, existing) =>
        {
            existing.callback += MediateReturn;
            return existing;
        });

        void MediateReturn(string? id, Bitmap? img)
        {
            onFetch?.Invoke(id, img);
        }
    }

    private static async void GameImageFetcher()
    {
        while (true)
        {
            await Task.Delay(10);

            if (queuedImageFetch.Count == 0)
                continue;

            IEnumerable<string> toClear = queuedImageFetch.Keys;

            await Parallel.ForEachAsync(toClear, async (string id, CancellationToken token) =>
            {
                if (!queuedImageFetch.TryGetValue(id, out ImageFetchRequest req))
                    return;

                if (!File.Exists(req.path))
                {
                    cachedImages.TryAdd(req.path, null);
                    return;
                }


                Bitmap bitmap = new Bitmap(req.path);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    req.callback?.Invoke(id, bitmap);
                });

                cachedImages.TryAdd(id, bitmap);
            });

            foreach (string i in toClear)
            {
                queuedImageFetch.TryRemove(i, out _);
            }
        }
    }

    private struct ImageFetchRequest
    {
        public string path;
        public FetchImageEvent callback;
    }
}
