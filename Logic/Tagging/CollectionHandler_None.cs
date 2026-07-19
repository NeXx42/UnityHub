using Models.Data;

namespace Logic.Tagging;

public class CollectionHandler_None : CollectionHandler_Base
{
    public override LoadRequest[] GetTransformations(ProjectInfo info) => [];
}
