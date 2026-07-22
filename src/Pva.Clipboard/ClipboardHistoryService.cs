using System.Collections.ObjectModel;

namespace Pva.Clipboard;

/// <summary>یک ورودی تاریخچه‌ی کلیپ‌بورد.</summary>
public sealed record ClipboardEntry
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string Text { get; init; } = string.Empty;

    public bool Pinned { get; init; }

    /// <summary>زمان به‌صورت ticks (توسط فراخوان تنظیم می‌شود؛ منطق به آن وابسته نیست).</summary>
    public long Timestamp { get; init; }
}

/// <summary>
/// منطق <b>خالص</b> تاریخچه‌ی کلیپ‌بورد: افزودن با حذف تکراری، سقف تعداد، pin (محافظت از
/// حذف)، جستجو و persistence. مستقل از Win32؛ مانیتور کلیپ‌بورد جدا این را تغذیه می‌کند.
/// </summary>
public sealed class ClipboardHistoryService
{
    private readonly ClipboardHistoryStore _store;
    private readonly int _maxItems;

    public ClipboardHistoryService(ClipboardHistoryStore store, int maxItems = 50)
    {
        _store = store;
        _maxItems = Math.Max(1, maxItems);
    }

    public ObservableCollection<ClipboardEntry> Items { get; } = new();

    public void Load()
    {
        Items.Clear();
        foreach (var entry in _store.LoadAll())
        {
            Items.Add(entry);
        }
    }

    /// <summary>یک متن جدید را بالای تاریخچه اضافه می‌کند (تکراری‌ها به بالا منتقل می‌شوند).</summary>
    public void Add(string text, long timestamp = 0)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var existing = Items.FirstOrDefault(e => string.Equals(e.Text, text, StringComparison.Ordinal));
        if (existing is not null)
        {
            Items.Remove(existing);
            Items.Insert(0, existing with { Timestamp = timestamp });
        }
        else
        {
            Items.Insert(0, new ClipboardEntry { Text = text, Timestamp = timestamp });
        }

        EvictOverflow();
        Save();
    }

    public IReadOnlyList<ClipboardEntry> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Items.ToList();
        }

        return Items.Where(e => e.Text.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public void TogglePin(string id)
    {
        var index = IndexOf(id);
        if (index >= 0)
        {
            Items[index] = Items[index] with { Pinned = !Items[index].Pinned };
            Save();
        }
    }

    public void Remove(string id)
    {
        var index = IndexOf(id);
        if (index >= 0)
        {
            Items.RemoveAt(index);
            Save();
        }
    }

    /// <summary>حذف موارد غیرِ pin‌شده.</summary>
    public void ClearUnpinned()
    {
        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (!Items[i].Pinned)
            {
                Items.RemoveAt(i);
            }
        }

        Save();
    }

    private void EvictOverflow()
    {
        // فقط موارد غیرِ pin از انتها حذف می‌شوند تا سقف رعایت شود.
        var unpinned = Items.Count(e => !e.Pinned);
        for (var i = Items.Count - 1; i >= 0 && unpinned > _maxItems; i--)
        {
            if (!Items[i].Pinned)
            {
                Items.RemoveAt(i);
                unpinned--;
            }
        }
    }

    private int IndexOf(string id)
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    private void Save() => _store.SaveAll(Items);
}
