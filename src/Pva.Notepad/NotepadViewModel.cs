using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Pva.Notepad;

/// <summary>
/// ViewModel نوت‌پد تب‌دار: مدیریت تب‌ها، باز/ذخیره، Word Wrap و بازیابی session. با
/// یک <see cref="NotepadSessionStore"/> تست‌پذیر ساخته می‌شود.
/// </summary>
public partial class NotepadViewModel : ObservableObject
{
    private readonly NotepadSessionStore _store;

    [ObservableProperty]
    private NotepadDocumentViewModel? _activeDocument;

    [ObservableProperty]
    private bool _wordWrap = true;

    public ObservableCollection<NotepadDocumentViewModel> Documents { get; } = new();

    public NotepadViewModel(NotepadSessionStore store)
    {
        _store = store;
        Restore();
    }

    [RelayCommand]
    private void New()
    {
        var document = new NotepadDocumentViewModel();
        Documents.Add(document);
        ActiveDocument = document;
    }

    [RelayCommand]
    private void CloseTab(NotepadDocumentViewModel? document)
    {
        document ??= ActiveDocument;
        if (document is null)
        {
            return;
        }

        Documents.Remove(document);
        if (Documents.Count == 0)
        {
            Documents.Add(new NotepadDocumentViewModel());
        }

        ActiveDocument = Documents[^1];
        SaveSession();
    }

    [RelayCommand]
    private void Open()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "فایل‌های متنی|*.txt;*.md;*.markdown|همه فایل‌ها|*.*",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var document = new NotepadDocumentViewModel(new NotepadDocument
        {
            Title = Path.GetFileName(dialog.FileName),
            FilePath = dialog.FileName,
            Content = File.ReadAllText(dialog.FileName),
        })
        {
            IsDirty = false,
        };

        Documents.Add(document);
        ActiveDocument = document;
        SaveSession();
    }

    [RelayCommand]
    private void Save()
    {
        var document = ActiveDocument;
        if (document is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(document.FilePath))
        {
            SaveAs();
            return;
        }

        File.WriteAllText(document.FilePath, document.Content);
        document.IsDirty = false;
        SaveSession();
    }

    [RelayCommand]
    private void SaveAs()
    {
        var document = ActiveDocument;
        if (document is null)
        {
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "متن|*.txt|Markdown|*.md",
            FileName = document.Title,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        File.WriteAllText(dialog.FileName, document.Content);
        document.FilePath = dialog.FileName;
        document.Title = Path.GetFileName(dialog.FileName);
        document.IsDirty = false;
        SaveSession();
    }

    /// <summary>وضعیت جاری (تب‌ها + محتوا) را برای بازیابی بعدی ذخیره می‌کند.</summary>
    public void SaveSession()
    {
        var activeIndex = ActiveDocument is null ? 0 : Math.Max(0, Documents.IndexOf(ActiveDocument));
        _store.Save(new NotepadSession
        {
            Documents = Documents.Select(d => d.ToModel()).ToList(),
            ActiveIndex = activeIndex,
        });
    }

    private void Restore()
    {
        var session = _store.Load();
        foreach (var document in session.Documents)
        {
            Documents.Add(new NotepadDocumentViewModel(document) { IsDirty = false });
        }

        if (Documents.Count == 0)
        {
            Documents.Add(new NotepadDocumentViewModel());
        }

        var index = Math.Clamp(session.ActiveIndex, 0, Documents.Count - 1);
        ActiveDocument = Documents[index];
    }
}
