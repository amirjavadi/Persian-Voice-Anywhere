using Microsoft.Extensions.DependencyInjection;

namespace Pva.StickyNotes;

/// <summary>
/// مدیریت پنجره‌های یادداشت چسبان: باز/بستن پنجره برای هر یادداشت و ذخیره‌ی موقعیت‌ها.
/// (لایه‌ی WPF؛ منطق داده در <see cref="StickyNotesService"/> است.)
/// </summary>
public sealed class StickyNotesManager
{
    private readonly StickyNotesService _service;
    private readonly Dictionary<string, StickyNoteWindow> _windows = new();

    public StickyNotesManager(StickyNotesService service) => _service = service;

    /// <summary>یادداشت‌های ذخیره‌شده را بارگذاری و نمایش می‌دهد.</summary>
    public void ShowAll()
    {
        _service.Load();
        foreach (var note in _service.Notes.ToList())
        {
            Open(note);
        }
    }

    /// <summary>یک یادداشت جدید می‌سازد و نمایش می‌دهد.</summary>
    public void CreateNew() => Open(_service.Add());

    /// <summary>ذخیره‌ی موقعیت/محتوای همه‌ی یادداشت‌ها.</summary>
    public void SaveAll() => _service.Save();

    private void Open(StickyNoteViewModel note)
    {
        if (_windows.TryGetValue(note.Id, out var existing))
        {
            existing.Activate();
            return;
        }

        var window = new StickyNoteWindow(note, OnDelete);
        window.Closed += (_, _) =>
        {
            _windows.Remove(note.Id);
            _service.Save();
        };

        _windows[note.Id] = window;
        window.Show();
    }

    private void OnDelete(StickyNoteViewModel note) => _service.Remove(note);
}

/// <summary>ثبت یادداشت‌های چسبان در DI.</summary>
public static class StickyNotesServiceCollectionExtensions
{
    public static IServiceCollection AddStickyNotes(this IServiceCollection services)
    {
        services.AddSingleton<StickyNotesStore>();
        services.AddSingleton<StickyNotesService>();
        services.AddSingleton<StickyNotesManager>();
        return services;
    }
}
