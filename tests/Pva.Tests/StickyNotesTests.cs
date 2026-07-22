using System.IO;
using Pva.StickyNotes;

namespace Pva.Tests;

/// <summary>تست store و service یادداشت‌های چسبان (منطق خالص).</summary>
public class StickyNotesTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"pva_sticky_{Guid.NewGuid():N}.json");

    private StickyNotesStore Store() => new(_path);

    [Fact]
    public void Store_RoundTrips()
    {
        var store = Store();
        store.SaveAll([
            new StickyNote { Content = "یادداشت اول", Left = 10, Top = 20, Pinned = true },
            new StickyNote { Content = "یادداشت دوم", Pinned = false },
        ]);

        var loaded = store.LoadAll();

        Assert.Equal(2, loaded.Count);
        Assert.Equal("یادداشت اول", loaded[0].Content);
        Assert.Equal(10, loaded[0].Left);
        Assert.False(loaded[1].Pinned);
    }

    [Fact]
    public void Load_Missing_ReturnsEmpty()
        => Assert.Empty(Store().LoadAll());

    [Fact]
    public void Service_Add_PersistsAndReloads()
    {
        var service = new StickyNotesService(Store());
        var note = service.Add();
        note.Content = "سلام";
        service.Save();

        var reloaded = new StickyNotesService(Store());
        reloaded.Load();

        Assert.Single(reloaded.Notes);
        Assert.Equal("سلام", reloaded.Notes[0].Content);
    }

    [Fact]
    public void Service_Remove_DeletesNote()
    {
        var service = new StickyNotesService(Store());
        var a = service.Add();
        service.Add();
        Assert.Equal(2, service.Notes.Count);

        service.Remove(a);
        Assert.Single(service.Notes);
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
