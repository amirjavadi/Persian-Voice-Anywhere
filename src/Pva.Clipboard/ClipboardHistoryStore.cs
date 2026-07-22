using System.IO;
using System.Text.Json;

namespace Pva.Clipboard;

/// <summary>ذخیره/بازیابی تاریخچه‌ی کلیپ‌بورد به JSON کنار exe.</summary>
public sealed class ClipboardHistoryStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public ClipboardHistoryStore(string? path = null)
        => Path = path ?? System.IO.Path.Combine(AppContext.BaseDirectory, "clipboard-history.json");

    public string Path { get; }

    public IReadOnlyList<ClipboardEntry> LoadAll()
    {
        if (!File.Exists(Path))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<List<ClipboardEntry>>(json, Options) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return [];
        }
    }

    public void SaveAll(IEnumerable<ClipboardEntry> entries)
    {
        var directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(Path, JsonSerializer.Serialize(entries.ToList(), Options));
    }
}
