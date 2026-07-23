using Models.Helpers;

namespace Models.Data;

public class EditorInfo
{
    public required string versionName { get; set; }

    public DateTime releaseDate { get; set; }
    public Download? download { get; set; }

    public string? stream { get; set; }
    public Label? label { get; set; }

    public TagData[] CreateTags()
    {
        List<TagData> tags = new(2);

        if (!string.IsNullOrEmpty(stream)) tags.Add(CreateTag(stream, "", stream));
        if (!string.IsNullOrEmpty(label?.labelText)) tags.Add(CreateTag(label.Value.labelText, label.Value.description, label.Value.colour ?? "WARNING"));

        return tags.ToArray();

        TagData CreateTag(string msg, string? desciption, string colourName)
        {
            string? hexColour = null;

            switch (colourName)
            {
                case "Beta":
                case "Alpha":
                    hexColour = "#006CDF";
                    break;

                case "WARNING":
                    hexColour = "#FFC53D";
                    break;

                case "ERROR":
                    hexColour = "#E5484D";
                    break;
            }

            return new TagData()
            {
                collectionId = -1,
                collectionName = msg,
                colour = hexColour ?? "#ffffff",
                tooltip = desciption
            };
        }
    }

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
            public required string id { get; set; }
            public string? slug { get; set; }
            public string? name { get; set; }
            public string? description { get; set; }
            public string? category { get; set; }
            public string? url { get; set; }
            public string? type { get; set; }

            public ulong? downloadSize { get; set; }
            public ulong? installedSize { get; set; }

            public bool? required { get; set; }
            public bool? hidden { get; set; }
            public bool? preSelected { get; set; }

            public string? integrity { get; set; }
            public string? destination { get; set; }

            public string getDownloadSize => downloadSize.FormatSize();
        }
    }
}
