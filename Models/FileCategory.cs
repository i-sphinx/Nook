using System.Collections.Generic;

namespace Nook.Models;

public enum CategoryType
{
    Images,
    Videos,
    Audio,
    Documents,
    Spreadsheets,
    Presentations,
    Code,
    Archives,
    Fonts,
    Others
}

public class FileCategory
{
    public CategoryType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string FolderName { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string AccentColor { get; init; } = string.Empty;
    public HashSet<string> Extensions { get; init; } = new();

    public static readonly List<FileCategory> All = new()
    {
        new FileCategory
        {
            Type = CategoryType.Images,
            Name = "Images",
            FolderName = "Images",
            Icon = "🖼️",
            AccentColor = "#6C63FF",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
                ".svg", ".ico", ".tiff", ".tif", ".heic", ".heif",
                ".raw", ".cr2", ".nef", ".arw", ".psd", ".xcf", ".avif"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Videos,
            Name = "Videos",
            FolderName = "Videos",
            Icon = "🎬",
            AccentColor = "#FF6584",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv",
                ".webm", ".m4v", ".3gp", ".ogv", ".ts", ".m2ts",
                ".vob", ".rmvb", ".divx"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Audio,
            Name = "Audio",
            FolderName = "Audio",
            Icon = "🎵",
            AccentColor = "#43D9A2",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma",
                ".m4a", ".opus", ".aiff", ".mid", ".midi", ".ape",
                ".alac", ".dsf"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Documents,
            Name = "Documents",
            FolderName = "Documents",
            Icon = "📄",
            AccentColor = "#FF9F43",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt",
                ".md", ".tex", ".epub", ".mobi", ".pages", ".wpd",
                ".log", ".nfo"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Spreadsheets,
            Name = "Spreadsheets",
            FolderName = "Spreadsheets",
            Icon = "📊",
            AccentColor = "#26de81",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".xls", ".xlsx", ".csv", ".ods", ".numbers", ".tsv"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Presentations,
            Name = "Presentations",
            FolderName = "Presentations",
            Icon = "📽️",
            AccentColor = "#fd9644",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".ppt", ".pptx", ".odp", ".key"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Code,
            Name = "Code",
            FolderName = "Code",
            Icon = "💻",
            AccentColor = "#45aaf2",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".cs", ".py", ".js", ".ts", ".html", ".css", ".cpp",
                ".c", ".h", ".java", ".go", ".rs", ".rb", ".php",
                ".swift", ".kt", ".sh", ".bat", ".ps1", ".json",
                ".xml", ".yaml", ".yml", ".toml", ".ini", ".cfg",
                ".sql", ".r", ".dart", ".lua", ".ex", ".exs"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Archives,
            Name = "Archives",
            FolderName = "Archives",
            Icon = "🗜️",
            AccentColor = "#a55eea",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2",
                ".xz", ".tar.gz", ".tar.bz2", ".tar.xz", ".iso",
                ".dmg", ".cab", ".deb", ".rpm", ".apk", ".ipa"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Fonts,
            Name = "Fonts",
            FolderName = "Fonts",
            Icon = "🔤",
            AccentColor = "#fc5c65",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
            {
                ".ttf", ".otf", ".woff", ".woff2", ".eot", ".fon"
            }
        },
        new FileCategory
        {
            Type = CategoryType.Others,
            Name = "Others",
            FolderName = "Others",
            Icon = "📦",
            AccentColor = "#778ca3",
            Extensions = new(System.StringComparer.OrdinalIgnoreCase)
        },
    };

    public static FileCategory GetCategory(string extension)
    {
        foreach (var cat in All)
        {
            if (cat.Type != CategoryType.Others && cat.Extensions.Contains(extension))
                return cat;
        }
        return All[^1]; // Others
    }
}
