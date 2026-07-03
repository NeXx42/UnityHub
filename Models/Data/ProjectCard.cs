namespace Models.Data;

public class ProjectCard
{
    public required int id;

    public required string name { get; set; }
    public required string directory { get; set; }
    public string? iconUrl { get; set; }
    public string? version { get; set; }

    public int[] tags { get; set; } = [];
    public int[] collections { get; set; } = [];
}