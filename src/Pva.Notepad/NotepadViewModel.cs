using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Pva.Notepad;

/// <summary>
/// ViewModel نوت‌پد تب‌دار: مدیریت تب‌ها، باز/ذخیره، Word Wrap، جهت متن و بازیابی session.
/// محتوای تب‌ها با یک تایمرِ debounce به‌صورت خودکار ذخیره می‌شود تا با بسته‌شدن ناگهانی،
/// یادداشت‌ها از دست نروند. با یک <see cref="NotepadSessionStore"/> تست‌پذیر ساخته می‌شود.
/// </summary>
public partial class NotepadViewModel : ObservableObject
{
    private readonly NotepadSessionStore _store;
    private readonly DispatcherTimer _autosaveTimer;

    [ObservableProperty]
    private NotepadDocumentViewModel? _activeDocument;

    [ObservableProperty]
    private bool _wordWrap = true;

    [ObservableProperty]
    private string? _statusMessage;

    public ObservableCollection<NotepadDocumentViewModel> Documents { get; } = new();

    /// <summary>
    /// پیش از دور انداختن یک سند ذخیره‌نشده فراخوانی می‌شود؛ اگر true برگرداند، بستن ادامه
    /// می‌یابد. View آن را به یک گفت‌وگوی تأیید وصل می‌کند؛ در تست null است (بدون مانع).
    /// </summary>
    public Func<NotepadDocumentViewModel, bool>? ConfirmDiscard { get; set; }

    public NotepadViewModel(NotepadSessionStore store)
    {
        _store = store;
        _autosaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
        _autosaveTimer.Tick += (_, _) =>
        {
            _autosaveTimer.Stop();
            SaveSession();
        };

        Documents.CollectionChanged += OnDocumentsChanged;
        Restore();
    }

    public bool HasUnsavedChanges => Documents.Any(d => d.IsDirty);

    [RelayCommand]
    private void New()
    {
        var document = new NotepadDocumentViewModel();
        Documents.Add(document);
        ActiveDocument = document;
        SaveSession();
    }

    [RelayCommand]
    private void CloseTab(NotepadDocumentViewModel? document)
    {
        document ??= ActiveDocument;
        if (document is null)
        {
            return;
        }

        // پیش از دور انداختن محتوای ذخیره‌نشده‌ی یک فایل، از کاربر تأیید بگیر.
        if (document.IsDirty && !string.IsNullOrEmpty(document.FilePath)
            && ConfirmDiscard is { } confirm && !confirm(document))
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

        try
        {
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
            StatusMessage = $"باز شد: {document.Title}";
            SaveSession();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusMessage = $"خطا در باز کردن فایل: {ex.Message}";
        }
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

        if (TryWrite(document.FilePath, document.Content))
        {
            document.IsDirty = false;
            StatusMessage = $"ذخیره شد: {document.Title}";
            SaveSession();
        }
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

        if (TryWrite(dialog.FileName, document.Content))
        {
            document.FilePath = dialog.FileName;
            document.Title = Path.GetFileName(dialog.FileName);
            document.IsDirty = false;
            StatusMessage = $"ذخیره شد: {document.Title}";
            SaveSession();
        }
    }

    /// <summary>جهت متنِ سند فعال را بین راست‌به‌چپ و چپ‌به‌راست جابجا می‌کند.</summary>
    [RelayCommand]
    private void ToggleDirection()
    {
        if (ActiveDocument is { } document)
        {
            document.IsRightToLeft = !document.IsRightToLeft;
            SaveSession();
        }
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

    private bool TryWrite(string path, string content)
    {
        try
        {
            File.WriteAllText(path, content);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusMessage = $"خطا در ذخیره‌ی فایل: {ex.Message}";
            return false;
        }
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

    // با تغییر محتوای هر سند، ذخیره‌ی خودکار را زمان‌بندی (debounce) می‌کنیم.
    private void OnDocumentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (NotepadDocumentViewModel doc in e.OldItems)
            {
                doc.PropertyChanged -= OnDocumentPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (NotepadDocumentViewModel doc in e.NewItems)
            {
                doc.PropertyChanged += OnDocumentPropertyChanged;
            }
        }
    }

    private void OnDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NotepadDocumentViewModel.Content))
        {
            _autosaveTimer.Stop();
            _autosaveTimer.Start();
        }
    }
}
