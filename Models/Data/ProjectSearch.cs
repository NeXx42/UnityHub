namespace Models.Data;

public class ProjectSearch
{
    public string? text;

    public IEnumerable<int>? tags;
    public IEnumerable<int>? collections;

    public int page;
    public int take;

    public int skip => page * take;

    public void Reset()
    {
        text = string.Empty;
        page = 0;
        tags = [];
        collections = [];
    }
}
