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
    private bool hasUpdate = false;

    public string? getCurrentVersion => currentVersion;
    public bool hasUpdateAvailable => hasUpdate;

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
            hasUpdate = await HasUpdate();
    }

    public async Task<bool> HasUpdate()
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
                    return !(el.GetString() ?? "").Equals(currentVersion ?? "UNIQUE", StringComparison.CurrentCultureIgnoreCase);
            }
        }
        catch (Exception e)
        {
            LoggingHelper.LogError(e);
        }

        return false;
    }
}
