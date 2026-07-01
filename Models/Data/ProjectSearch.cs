namespace Models.Data;

public class ProjectSearch
{
    public string? text;

    public int[]? tags;
    public int[]? collections;

    public int page;
    public int take;

    public int skip => page * take;
}
