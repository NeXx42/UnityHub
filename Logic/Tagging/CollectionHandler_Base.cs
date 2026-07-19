using Models.Data;

namespace Logic.Tagging;

public abstract class CollectionHandler_Base
{
    public virtual string? getConfirmationMessage => string.Empty;
    public abstract LoadRequest[] GetTransformations(ProjectInfo info);
}
