using System.Reflection;

namespace Logic.Versioning;

public class Versioning_AppImage : VersionLogic
{
    public Versioning_AppImage(IEnumerable<AssemblyMetadataAttribute> metadataAttributes) : base(metadataAttributes)
    {
    }
}
