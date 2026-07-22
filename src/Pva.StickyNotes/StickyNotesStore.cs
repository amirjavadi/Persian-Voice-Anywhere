using System.IO;
using System.Text.Json;

namespace Pva.StickyNotes;

/// <summary>یک یادداشت چسبان (پرتابل، در JSON کنار exe ذخیره می‌شود).</summary>
public sealed record StickyNote
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string Content { get; init; } = string.Empty;

    public double Left { get; init; } = 120;

    public double Top { get; init; } = 120;

    public double Width { get; init; } = 240;

    public double Height { get; init; } = 220;

    public bool Pinned { get; init; } = true;
}

/// <summary>
/// ذخیره/بازیابی <b>خالص</b> یادداشت‌های چسبان به JSON. (SQLite در backlog؛ برای تعداد
/// کم یادداشت، JSON پرتابل و کافی است.)
/// </summary>
public sealed class StickyNotesStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public StickyNotesStore(string? path = null)
        => Path = path ?? System.IO.Path.Combine(AppContext.BaseDirectory, "sticky-notes.json");

    public string Path { get; }

    public IReadOnlyList<StickyNote> LoadAll()
    {
        if (!File.Exists(Path))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<List<StickyNote>>(json, Options) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return [];
        }
    }

    public void SaveAll(IEnumerable<StickyNote> notes)
    {
        var directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(Path, JsonSerializer.Serialize(notes.ToList(), Options));
    }
}
