namespace Models.Data;

public class EditorInfo
{
    public required string versionName { get; set; }

    public DateTime releaseDate { get; set; }
    public Download? download { get; set; }

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
        public Module[] modules { get; set; }

        public struct Module
        {
            public string? id { get; set; }
            public string? slug { get; set; }
            public string? name { get; set; }
            public string? description { get; set; }
            public string? url { get; set; }
            public string? type { get; set; }

            public ulong? downloadSize { get; set; }
            public ulong? installedSize { get; set; }

            public bool? required { get; set; }
            public bool? hidden { get; set; }
            public bool? preSelected { get; set; }

            public string? integrity { get; set; }
            public string? destination { get; set; }
        }
    }
}
