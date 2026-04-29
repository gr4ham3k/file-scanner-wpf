public static class FileTypeCatalog
{
    public static readonly Dictionary<string, string[]> Groups = new()
    {
        ["All"] = [],
        ["Executables"] = [".exe", ".msi", ".bat"],
        ["Documents"] = [".pdf", ".docx", ".txt"],
        ["Images"] = [".jpg", ".png", ".bmp", ".gif"],
        ["Videos"] = [".mp4", ".avi", ".mkv"]
    };
}