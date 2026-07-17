namespace Models.Data;

public class CollectionData
{
    public required string type { get; set; }

    public required int collectionId { get; set; }
    public required string collectionName { get; set; }
    public string? colour { get; set; }

    public string? bgColour => string.IsNullOrEmpty(colour) ? null : $"#{0x28:X2}{colour.Substring(1)}";
}
