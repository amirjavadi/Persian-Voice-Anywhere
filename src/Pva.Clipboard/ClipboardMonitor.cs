using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Pva.Clipboard;

/// <summary>
/// شنود تغییرات کلیپ‌بورد ویندوز با AddClipboardFormatListener روی یک پنجره‌ی
/// message-only. هنگام کپی متن، رویداد <see cref="TextCopied"/> صادر می‌شود. روی نخ UI
/// (STA) اجرا می‌شود. تأیید نهایی دستی.
/// </summary>
public sealed class ClipboardMonitor : IDisposable
{
    private const int WmClipboardUpdate = 0x031D;
    private static readonly nint HwndMessage = -3;

    private HwndSource? _source;

    public event Action<string>? TextCopied;

    public void Start()
    {
        if (_source is not null)
        {
            return;
        }

        var parameters = new HwndSourceParameters("pva-clipboard-monitor")
        {
            ParentWindow = HwndMessage, // پنجره‌ی message-only
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
        AddClipboardFormatListener(_source.Handle);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WmClipboardUpdate && System.Windows.Clipboard.ContainsText())
        {
            try
            {
                var text = System.Windows.Clipboard.GetText();
                if (!string.IsNullOrEmpty(text))
                {
                    TextCopied?.Invoke(text);
                }
            }
            catch (COMException)
            {
                // کلیپ‌بورد موقتاً قفل است؛ نادیده بگیر.
            }
        }

        return nint.Zero;
    }

    public void Dispose()
    {
        if (_source is not null)
        {
            RemoveClipboardFormatListener(_source.Handle);
            _source.RemoveHook(WndProc);
            _source.Dispose();
            _source = null;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListener(nint hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveClipboardFormatListener(nint hwnd);
}
