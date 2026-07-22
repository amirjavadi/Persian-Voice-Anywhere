using System.IO;
using Pva.Storage;

namespace Pva.Tests;

/// <summary>تست round-trip تنظیمات JSON.</summary>
public class AppSettingsStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"pva_settings_{Guid.NewGuid():N}.json");

    [Fact]
    public void SaveThenLoad_RoundTripsValues()
    {
        var store = new JsonSettingsStore(_path);
        var settings = new AppSettings
        {
            HotkeyGesture = "DoubleCtrl",
            HotkeyMode = "Toggle",
            PreferredEngine = "FasterWhisper",
            MicOpacity = 0.5,
            UsePersianDigits = false,
        };

        store.Save(settings);
        var loaded = store.Load();

        Assert.Equal(settings, loaded);
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        var store = new JsonSettingsStore(_path);
        Assert.Equal(new AppSettings(), store.Load());
    }

    [Fact]
    public void Load_CorruptFile_ReturnsDefaults()
    {
        File.WriteAllText(_path, "{ not valid json ");
        var store = new JsonSettingsStore(_path);
        Assert.Equal(new AppSettings(), store.Load());
    }

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }

        GC.SuppressFinalize(this);
    }
}
