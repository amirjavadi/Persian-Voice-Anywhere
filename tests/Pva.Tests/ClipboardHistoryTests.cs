using System.IO;
using Pva.Clipboard;

namespace Pva.Tests;

/// <summary>تست منطق خالص تاریخچه‌ی کلیپ‌بورد + store.</summary>
public class ClipboardHistoryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"pva_clip_{Guid.NewGuid():N}.json");

    private ClipboardHistoryService Service(int max = 50) => new(new ClipboardHistoryStore(_path), max);

    [Fact]
    public void Add_InsertsAtTop()
    {
        var service = Service();
        service.Add("اول");
        service.Add("دوم");

        Assert.Equal("دوم", service.Items[0].Text);
        Assert.Equal("اول", service.Items[1].Text);
    }

    [Fact]
    public void Add_Duplicate_MovesToTopWithoutDuplicating()
    {
        var service = Service();
        service.Add("a");
        service.Add("b");
        service.Add("a");

        Assert.Equal(2, service.Items.Count);
        Assert.Equal("a", service.Items[0].Text);
    }

    [Fact]
    public void Cap_EvictsOldestUnpinned()
    {
        var service = Service(max: 2);
        service.Add("۱");
        service.Add("۲");
        service.Add("۳");

        Assert.Equal(2, service.Items.Count);
        Assert.DoesNotContain(service.Items, e => e.Text == "۱");
    }

    [Fact]
    public void Pinned_SurvivesCap()
    {
        var service = Service(max: 2);
        service.Add("مهم");
        service.TogglePin(service.Items[0].Id);
        service.Add("a");
        service.Add("b");
        service.Add("c");

        Assert.Contains(service.Items, e => e.Text == "مهم" && e.Pinned);
    }

    [Fact]
    public void Search_FiltersByContains()
    {
        var service = Service();
        service.Add("سلام دنیا");
        service.Add("خداحافظ");

        var results = service.Search("دنیا");
        Assert.Single(results);
        Assert.Equal("سلام دنیا", results[0].Text);
    }

    [Fact]
    public void ClearUnpinned_KeepsPinned()
    {
        var service = Service();
        service.Add("پین");
        service.TogglePin(service.Items[0].Id);
        service.Add("معمولی");

        service.ClearUnpinned();

        Assert.Single(service.Items);
        Assert.Equal("پین", service.Items[0].Text);
    }

    [Fact]
    public void Whitespace_IsIgnored()
    {
        var service = Service();
        service.Add("   ");
        Assert.Empty(service.Items);
    }

    [Fact]
    public void Persistence_ReloadsAcrossInstances()
    {
        var first = Service();
        first.Add("ماندگار");

        var second = Service();
        second.Load();

        Assert.Contains(second.Items, e => e.Text == "ماندگار");
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
