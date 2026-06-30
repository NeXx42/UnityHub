using Models.Helpers;

namespace Models.Data;

public enum RenderPipelineTypes
{
    Universal_Render_Pipeline,
    High_Definition_Render_Pipeline,
    Built_In_Render_Pipeline
}

public class ProjectInfo
{
    public required int id;

    public required string name { get; set; }
    public required string directory { get; set; }
    public string? iconUrl { get; set; }

    public string? version { get; set; }
    public RenderPipelineTypes? renderPipeline { get; set; }
    public string renderPipelineName => renderPipeline.HasValue ? renderPipeline.Value.GetDisplayName() : "";

    public long? size { get; set; }
    public int? packages { get; set; }
}
