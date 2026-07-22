using System.IO;
using System.Text.Json;

namespace Pva.TextExpansion;

/// <summary>ذخیره/بازیابی میان‌بُرها به JSON کنار exe. نبود/خرابی فایل ⇒ پیش‌فرض‌ها.</summary>
public sealed class ExpansionStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public ExpansionStore(string? path = null)
        => Path = path ?? System.IO.Path.Combine(AppContext.BaseDirectory, "text-expansions.json");

    public string Path { get; }

    public IReadOnlyDictionary<string, string> Load()
    {
        if (!File.Exists(Path))
        {
            return TextExpansionDefaults.Expansions;
        }

        try
        {
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, Options)
                ?? new Dictionary<string, string>();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return TextExpansionDefaults.Expansions;
        }
    }

    public void Save(IReadOnlyDictionary<string, string> expansions)
    {
        var directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(Path, JsonSerializer.Serialize(expansions, Options));
    }
}
