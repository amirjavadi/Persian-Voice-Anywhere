using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pva.StickyNotes;

/// <summary>ViewModel یک یادداشت چسبان.</summary>
public partial class StickyNoteViewModel : ObservableObject
{
    [ObservableProperty]
    private string _content;

    [ObservableProperty]
    private double _left;

    [ObservableProperty]
    private double _top;

    [ObservableProperty]
    private double _width;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private bool _pinned;

    public StickyNoteViewModel(StickyNote note)
    {
        Id = note.Id;
        _content = note.Content;
        _left = note.Left;
        _top = note.Top;
        _width = note.Width;
        _height = note.Height;
        _pinned = note.Pinned;
    }

    public string Id { get; }

    public StickyNote ToModel() => new()
    {
        Id = Id,
        Content = Content,
        Left = Left,
        Top = Top,
        Width = Width,
        Height = Height,
        Pinned = Pinned,
    };
}

/// <summary>
/// مدیریت مجموعه‌ی یادداشت‌های چسبان + persistence. منطق خالص (بدون WPF) تا تست‌پذیر باشد؛
/// نمایش پنجره‌ها بر عهده‌ی <c>StickyNotesManager</c> است.
/// </summary>
public sealed class StickyNotesService
{
    private readonly StickyNotesStore _store;

    public StickyNotesService(StickyNotesStore store) => _store = store;

    public ObservableCollection<StickyNoteViewModel> Notes { get; } = new();

    public void Load()
    {
        Notes.Clear();
        foreach (var note in _store.LoadAll())
        {
            Notes.Add(new StickyNoteViewModel(note));
        }
    }

    public StickyNoteViewModel Add()
    {
        var note = new StickyNoteViewModel(new StickyNote());
        Notes.Add(note);
        Save();
        return note;
    }

    public void Remove(StickyNoteViewModel note)
    {
        Notes.Remove(note);
        Save();
    }

    public void Save() => _store.SaveAll(Notes.Select(n => n.ToModel()));
}
