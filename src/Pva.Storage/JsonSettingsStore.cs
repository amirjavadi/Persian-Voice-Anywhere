using System.IO;
using System.Text.Json;

namespace Pva.Storage;

/// <summary>خواندن/نوشتن تنظیمات کاربر.</summary>
public interface ISettingsStore
{
    string Path { get; }

    AppSettings Load();

    void Save(AppSettings settings);
}

/// <summary>
/// ذخیره‌ی تنظیمات به‌صورت JSON کنار فایل اجرایی. اگر فایل نباشد یا خراب باشد، مقادیر
/// پیش‌فرض برگردانده می‌شود (اجرا هرگز به‌خاطر تنظیمات نمی‌شکند).
/// </summary>
public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public JsonSettingsStore(string? path = null)
        => Path = path ?? System.IO.Path.Combine(AppContext.BaseDirectory, "settings.json");

    public string Path { get; }

    public AppSettings Load()
    {
        if (!File.Exists(Path))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(Path, JsonSerializer.Serialize(settings, Options));
    }
}
