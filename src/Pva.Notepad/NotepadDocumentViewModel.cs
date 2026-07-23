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
    [NotifyPropertyChangedFor(nameof(TabHeader))]
    private bool _isDirty;

    [ObservableProperty]
    private bool _isRightToLeft;

    public NotepadDocumentViewModel(NotepadDocument document)
    {
        _title = document.Title;
        _content = document.Content;
        _filePath = document.FilePath;
        _isRightToLeft = document.IsRightToLeft;
    }

    public NotepadDocumentViewModel()
        : this(new NotepadDocument())
    {
    }

    /// <summary>عنوان تب همراه با نشانگر «ذخیره‌نشده» (●).</summary>
    public string TabHeader => IsDirty ? $"● {Title}" : Title;

    /// <summary>تعداد کاراکترها (برای نوار وضعیت).</summary>
    public int CharCount => Content.Length;

    /// <summary>تعداد واژه‌ها (تقریبی، بر پایه‌ی فاصله‌ها).</summary>
    public int WordCount => Content
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        .Length;

    partial void OnTitleChanged(string value) => OnPropertyChanged(nameof(TabHeader));

    partial void OnContentChanged(string value)
    {
        IsDirty = true;
        OnPropertyChanged(nameof(CharCount));
        OnPropertyChanged(nameof(WordCount));
    }

    public NotepadDocument ToModel() => new()
    {
        Title = Title,
        Content = Content,
        FilePath = FilePath,
        IsRightToLeft = IsRightToLeft,
    };
}
