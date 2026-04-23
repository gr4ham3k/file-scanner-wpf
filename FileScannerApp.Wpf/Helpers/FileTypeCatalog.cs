namespace FileScannerApp.Wpf.Helpers;

public sealed class FileTypeFilter
{
    public string Name { get; }
    public IReadOnlyCollection<string> Extensions { get; }

    public FileTypeFilter(string name, params string[] extensions)
    {
        Name = name;
        Extensions = extensions;
    }

    public override string ToString() => Name;
}

public static class FileTypeCatalog
{
    public static readonly IReadOnlyList<FileTypeFilter> Filters =
    [
        new("All files"),
        new("Music", ".mp3", ".wav"),
        new("Images", ".jpg", ".png", ".bmp", ".gif"),
        new("Documents", ".pdf", ".docx", ".txt"),
        new("Videos", ".mp4", ".avi", ".mkv")
    ];

    public static readonly IReadOnlyDictionary<string, string[]> Groups = new Dictionary<string, string[]>
    {
        ["Executables"] = [".exe", ".msi", ".bat"],
        ["Documents"] = [".pdf", ".docx", ".txt"],
        ["Images"] = [".jpg", ".png", ".bmp", ".gif"],
        ["Videos"] = [".mp4", ".avi", ".mkv"]
    };
}
