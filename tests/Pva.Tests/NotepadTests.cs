using System.IO;
using Pva.Notepad;

namespace Pva.Tests;

/// <summary>تست session-store و ViewModel نوت‌پد (منطق خالص، بدون UI).</summary>
public class NotepadTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"pva_notepad_{Guid.NewGuid():N}.json");

    private NotepadSessionStore Store() => new(_path);

    [Fact]
    public void Session_SaveThenLoad_RoundTrips()
    {
        var store = Store();
        var session = new NotepadSession
        {
            ActiveIndex = 1,
            Documents =
            [
                new NotepadDocument { Title = "یک", Content = "سلام" },
                new NotepadDocument { Title = "دو", Content = "دنیا", FilePath = @"C:\x.txt" },
            ],
        };

        store.Save(session);
        var loaded = store.Load();

        Assert.Equal(2, loaded.Documents.Count);
        Assert.Equal(1, loaded.ActiveIndex);
        Assert.Equal("دنیا", loaded.Documents[1].Content);
        Assert.Equal(@"C:\x.txt", loaded.Documents[1].FilePath);
    }

    [Fact]
    public void Load_Missing_ReturnsEmptySession()
        => Assert.Empty(Store().Load().Documents);

    [Fact]
    public void NewViewModel_WithNoSession_StartsWithOneTab()
    {
        var vm = new NotepadViewModel(Store());
        Assert.Single(vm.Documents);
        Assert.NotNull(vm.ActiveDocument);
    }

    [Fact]
    public void NewCommand_AddsTab_CloseKeepsAtLeastOne()
    {
        var vm = new NotepadViewModel(Store());
        vm.NewCommand.Execute(null);
        Assert.Equal(2, vm.Documents.Count);

        vm.CloseTabCommand.Execute(vm.ActiveDocument);
        Assert.Single(vm.Documents);

        // بستن آخرین تب باز هم یک تب خالی نگه می‌دارد.
        vm.CloseTabCommand.Execute(vm.ActiveDocument);
        Assert.Single(vm.Documents);
    }

    [Fact]
    public void SessionRestore_BringsBackContentAcrossInstances()
    {
        var store = Store();
        var first = new NotepadViewModel(store);
        first.ActiveDocument!.Content = "متن ذخیره‌شده";
        first.SaveSession();

        var second = new NotepadViewModel(store);
        Assert.Contains(second.Documents, d => d.Content == "متن ذخیره‌شده");
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
