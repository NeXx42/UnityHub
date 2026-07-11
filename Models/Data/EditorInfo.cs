namespace Models.Data;

public class EditorInfo
{
    public required string versionName { get; set; }

    public DateTime releaseDate { get; set; }
    public Download[] downloads { get; set; } = [];

    public string? stream { get; set; }
    public Label? label { get; set; }

    public struct Label
    {
        public string? description { get; set; }
        public string? labelText { get; set; }
        public string? colour { get; set; }
        public string? icon { get; set; }
    }

    public struct Download
    {
        public string? url { get; set; }
        public string? type { get; set; }
        public string? platform { get; set; }
        public string? architecture { get; set; }

        public ulong downloadSize { get; set; }
        public ulong installSize { get; set; }

        public string? integrity { get; set; }
    }
}
