namespace Models.Data;

public class EditorInfo
{
    public required string versionName;

    public DateTime releaseDate;
    public Download[] downloads = [];

    public string? stream;
    public Label? label;

    public struct Label
    {
        public string? description;
        public string? labelText;
        public string? colour;
        public string? icon;
    }

    public struct Download
    {
        public string? url;
        public string? type;
        public string? platform;
        public string? architecture;

        public ulong downloadSize;
        public ulong installSize;

        public string? integrity;
    }
}
