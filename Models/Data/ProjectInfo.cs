namespace Models.Data;

public class ProjectInfo
{
    public required int id;

    public required string name { get; set; }
    public required string directory { get; set; }
    public string? iconUrl { get; set; }
}
