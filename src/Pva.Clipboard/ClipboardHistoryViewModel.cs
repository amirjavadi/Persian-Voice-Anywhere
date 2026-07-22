using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Pva.Clipboard;

/// <summary>ViewModel پنجره‌ی تاریخچه‌ی کلیپ‌بورد: جستجو، کپی، pin، حذف.</summary>
public partial class ClipboardHistoryViewModel : ObservableObject
{
    private readonly ClipboardHistoryService _service;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public ClipboardHistoryViewModel(ClipboardHistoryService service)
    {
        _service = service;
        _service.Items.CollectionChanged += OnItemsChanged;
        Refresh();
    }

    public ObservableCollection<ClipboardEntry> Results { get; } = new();

    partial void OnSearchQueryChanged(string value) => Refresh();

    [RelayCommand]
    private void Copy(ClipboardEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        try
        {
            System.Windows.Clipboard.SetText(entry.Text);
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // کلیپ‌بورد موقتاً قفل است.
        }
    }

    [RelayCommand]
    private void Pin(ClipboardEntry? entry)
    {
        if (entry is not null)
        {
            _service.TogglePin(entry.Id);
            Refresh();
        }
    }

    [RelayCommand]
    private void Delete(ClipboardEntry? entry)
    {
        if (entry is not null)
        {
            _service.Remove(entry.Id);
            Refresh();
        }
    }

    [RelayCommand]
    private void ClearUnpinned()
    {
        _service.ClearUnpinned();
        Refresh();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private void Refresh()
    {
        Results.Clear();
        foreach (var entry in _service.Search(SearchQuery))
        {
            Results.Add(entry);
        }
    }
}
