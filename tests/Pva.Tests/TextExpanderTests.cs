using System.IO;
using Pva.TextExpansion;

namespace Pva.Tests;

/// <summary>تست گسترش متن (منطق خالص) و store.</summary>
public class TextExpanderTests
{
    private static TextExpander Expander(Dictionary<string, string> map) => new(map);

    [Fact]
    public void ShortcutToken_IsExpanded()
    {
        var expander = Expander(new() { ["/phone"] = "۰۹۱۲۳۴۵۶۷۸۹" });
        Assert.Equal("شماره من ۰۹۱۲۳۴۵۶۷۸۹ است", expander.Expand("شماره من /phone است"));
    }

    [Fact]
    public void MultiWordMacro_IsExpanded()
    {
        var expander = Expander(new() { ["امضا"] = "با احترام،\nامیر جوادی" });
        Assert.Equal("با احترام،\nامیر جوادی", expander.Expand("امضا"));
    }

    [Fact]
    public void NonTriggerText_IsUnchanged()
    {
        var expander = Expander(new() { ["/phone"] = "x" });
        Assert.Equal("این یک جمله است", expander.Expand("این یک جمله است"));
    }

    [Fact]
    public void ArabicVariant_StillMatches()
    {
        // تریگر با ي عربی هم باید نگاشت شود (نرمال‌سازی).
        var expander = Expander(new() { ["ايميل"] = "a@b.com" });
        Assert.Equal("a@b.com", expander.Expand("ایمیل".Replace('ی', 'ي')));
    }

    [Fact]
    public void EmptyOrNoExpansions_ReturnsInput()
    {
        Assert.Equal("سلام", new TextExpander(new Dictionary<string, string>()).Expand("سلام"));
        Assert.Equal(string.Empty, Expander(new() { ["/x"] = "y" }).Expand(string.Empty));
    }

    [Fact]
    public void Store_RoundTrips()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pva_exp_{Guid.NewGuid():N}.json");
        try
        {
            var store = new ExpansionStore(path);
            store.Save(new Dictionary<string, string> { ["/sig"] = "امیر" });

            var loaded = store.Load();
            Assert.Equal("امیر", loaded["/sig"]);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void Store_Missing_ReturnsDefaults()
    {
        var store = new ExpansionStore(Path.Combine(Path.GetTempPath(), $"pva_exp_{Guid.NewGuid():N}.json"));
        Assert.NotEmpty(store.Load());
    }
}
