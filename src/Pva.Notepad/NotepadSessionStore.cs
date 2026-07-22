using System.IO;
using System.Text.Json;

namespace Pva.Notepad;

/// <summary>یک سند نوت‌پد برای ذخیره/بازیابی session.</summary>
public sealed record NotepadDocument
{
    public string Title { get; init; } = "بدون‌عنوان";

    public string? FilePath { get; init; }

    public string Content { get; init; } = string.Empty;
}

/// <summary>وضعیت session نوت‌پد: تب‌های باز + تب فعال.</summary>
public sealed record NotepadSession
{
    public IReadOnlyList<NotepadDocument> Documents { get; init; } = [];

    public int ActiveIndex { get; init; }
}

/// <summary>
/// ذخیره/بازیابی <b>خالص</b> session نوت‌پد به JSON کنار فایل اجرایی. اگر فایل نباشد یا
/// خراب باشد، یک session خالی برمی‌گرداند.
/// </summary>
public sealed class NotepadSessionStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public NotepadSessionStore(string? path = null)
        => Path = path ?? System.IO.Path.Combine(AppContext.BaseDirectory, "notepad-session.json");

    public string Path { get; }

    public NotepadSession Load()
    {
        if (!File.Exists(Path))
        {
            return new NotepadSession();
        }

        try
        {
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<NotepadSession>(json, Options) ?? new NotepadSession();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return new NotepadSession();
        }
    }

    public void Save(NotepadSession session)
    {
        var directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(Path, JsonSerializer.Serialize(session, Options));
    }
}
