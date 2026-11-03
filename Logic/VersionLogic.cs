using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using Models;
using Models.Enums;
using Models.Helpers;
using Models.Interfaces;

namespace Logic;

public abstract class VersionLogic : IVersionLogic
{
    private readonly string? currentVersion;
    private readonly string? gitSha;

    private string? latestVersion;

    public string? getCurrentVersion => currentVersion;
    public string? getLatestVersion => latestVersion;

    public bool hasUpdateAvailable => !string.IsNullOrEmpty(latestVersion) && currentVersion != latestVersion;

    public VersionLogic(IEnumerable<AssemblyMetadataAttribute> metadataAttributes)
    {
        currentVersion = metadataAttributes.FirstOrDefault(m => m.Key.Equals("GitVersion"))?.Value;
        gitSha = metadataAttributes.FirstOrDefault(m => m.Key.Equals("GitSha"))?.Value;

        _ = AutoCheckForUpdates();
    }

    private async Task AutoCheckForUpdates()
    {
        var check = await DependencyManager.GetService<IConfigLogic>()!.Get<Config_EnabledStatus>(ConfigEntry.AutoCheckUpdates, Config_EnabledStatus.Enabled);

        if (check == Config_EnabledStatus.Enabled)
            latestVersion = await GetLatestVersion();
    }

    public async Task<string?> GetLatestVersion()
    {
        try
        {
            const string url = $"https://api.github.com/repos/NeXx42/unityhub/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"{GlobalConfig.APPLICATION_NAME}/updateChecker");

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage res = await client.SendAsync(msg);

                res.EnsureSuccessStatusCode();

                JsonDocument json = await JsonDocument.ParseAsync(res.Content.ReadAsStream());

                if (json.RootElement.TryGetProperty("tag_name", out JsonElement el))
                    return el.GetString();
            }
        }
        catch (Exception e)
        {
            LoggingHelper.LogError(e);
        }

        return string.Empty;
    }

    public bool GetUpdateInfo(out string versionName, out string updateMessage)
    {
        versionName = latestVersion ?? string.Empty;
        updateMessage = latestVersion ?? "update exists";

        return hasUpdateAvailable;
    }
}
