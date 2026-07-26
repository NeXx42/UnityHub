using System.Reflection;
using Models.Interfaces;

namespace Logic;

public abstract class VersionLogic : IVersionLogic
{
    private readonly string? currentVersion;
    private readonly string? gitSha;

    public string? getCurrentVersion => currentVersion;

    public VersionLogic(IEnumerable<AssemblyMetadataAttribute> metadataAttributes)
    {
        currentVersion = metadataAttributes.FirstOrDefault(m => m.Key.Equals("GitVersion"))?.Value;
        gitSha = metadataAttributes.FirstOrDefault(m => m.Key.Equals("GitSha"))?.Value;
    }
}
