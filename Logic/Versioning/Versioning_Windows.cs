using System.Reflection;

namespace Logic.Versioning;

public class Versioning_Windows : VersionLogic
{
    public Versioning_Windows(IEnumerable<AssemblyMetadataAttribute> metadataAttributes) : base(metadataAttributes)
    {
    }
}
