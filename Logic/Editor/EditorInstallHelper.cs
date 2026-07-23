using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Logic.Editor;

public static class EditorInstallHelper
{
    public static async Task DownloadFile(string url, string resultPath, IProgress<float> subProgress, CancellationToken token)
    {
        using (HttpClient http = new HttpClient())
        {
            HttpResponseMessage res = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            res.EnsureSuccessStatusCode();

            long? totalBytes = res.Content.Headers.ContentLength;

            await using Stream stream = await res.Content.ReadAsStreamAsync(token);
            await using FileStream file = File.Create(resultPath);

            byte[] buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                if (token.IsCancellationRequested)
                    break;

                await file.WriteAsync(buffer, 0, bytesRead);

                totalRead += bytesRead;

                if (totalBytes.HasValue)
                {
                    subProgress?.Report((float)totalRead / totalBytes.Value);
                }
            }

            subProgress?.Report(1f);
        }
    }

    public static async Task Extract(string path, string result, CancellationToken cancellationToken, IProgress<float> progress)
    {
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "7z";

        info.RedirectStandardError = true;
        info.RedirectStandardOutput = true;
        info.UseShellExecute = false;
        info.CreateNoWindow = true;

        info.ArgumentList.Add("x");
        info.ArgumentList.Add(path);

        info.ArgumentList.Add($"-o{result}");

        info.ArgumentList.Add("-y");
        info.ArgumentList.Add("-bsp1");

        Process p = new Process();
        p.StartInfo = info;

        p.Start();
        await ReadProgressOfExtraction(p);

        if (p.ExitCode != 0)
        {
            throw new Exception(await p.StandardError.ReadToEndAsync());
        }

        Task ReadProgressOfExtraction(Process p)
        {
            int charNumber;
            const int newLineCharNumber = '\b';

            string line = string.Empty;

            TaskCompletionSource task = new TaskCompletionSource();

            Task.Run(() =>
            {
                while (!p.HasExited)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        p.Kill();
                        return;
                    }

                    while ((charNumber = p.StandardOutput.Read()) != -1)
                    {
                        try
                        {
                            if (charNumber == newLineCharNumber)
                            {
                                string percentageText = line.Replace(" ", "");
                                Match match = Regex.Match(percentageText, @"^(\d+)%");

                                if (match.Success)
                                {
                                    int percentage = int.Parse(match.Groups[1].Value);
                                    progress.Report(percentage / 100f);
                                }

                                line = string.Empty;
                            }
                            else
                            {
                                line += (char)charNumber;
                            }
                        }
                        catch { }
                    }
                }

                task.SetResult();
            });

            return task.Task;
        }
    }
}
