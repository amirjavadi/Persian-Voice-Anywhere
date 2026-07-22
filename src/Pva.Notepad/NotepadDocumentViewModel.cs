using CommunityToolkit.Mvvm.ComponentModel;

namespace Pva.Notepad;

/// <summary>ViewModel یک تب/سند نوت‌پد.</summary>
public partial class NotepadDocumentViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _content;

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private bool _isDirty;

    public NotepadDocumentViewModel(NotepadDocument document)
    {
        _title = document.Title;
        _content = document.Content;
        _filePath = document.FilePath;
    }

    public NotepadDocumentViewModel()
        : this(new NotepadDocument())
    {
    }

    partial void OnContentChanged(string value) => IsDirty = true;

    public NotepadDocument ToModel() => new()
    {
        Title = Title,
        Content = Content,
        FilePath = FilePath,
    };
}
