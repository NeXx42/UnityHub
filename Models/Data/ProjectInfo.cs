using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Models.Enums;
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
    public string getSizeTxt => size.FormatSize();

    public long? lastOpened { get; set; }
    public string getLastOpenedTxt => lastOpened.HasValue ? DateTimeOffset.FromUnixTimeSeconds(lastOpened.Value).LocalDateTime.ToString() : "Never";

    public long? created { get; set; }
    public string getCreatedTxt => created.HasValue ? DateTimeOffset.FromUnixTimeSeconds(created.Value).LocalDateTime.ToString() : "Never";

    public string? notes { get; set; }
    public int? packages { get; set; }
    public bool favourited { get; set; }

    public HashSet<int> tags { get; set; } = [];
    public required int collectionId { get; set; }

    public static ProjectInfo Test => new ProjectInfo()
    {
        name = "Test",
        directory = "test",
        id = 0,
        collectionId = (int)DefaultCollectionIds.InDevelopment
    };
}
