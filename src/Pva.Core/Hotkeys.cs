namespace Pva.Core;

/// <summary>نحوه‌ی رفتار یک کلید میانبر.</summary>
public enum HotkeyMode
{
    /// <summary>تا وقتی کلید نگه داشته شود، ضبط ادامه دارد.</summary>
    PushToTalk,

    /// <summary>هر فشار، ضبط را روشن/خاموش می‌کند.</summary>
    Toggle,
}

/// <summary>یک اتصال کلید میانبر قابل‌تنظیم توسط کاربر.</summary>
public sealed record HotkeyBinding
{
    public required string Name { get; init; }

    /// <summary>توصیف کلید(ها)، مثل "Ctrl+Space" یا "CapsLock" یا "DoubleCtrl".</summary>
    public required string Gesture { get; init; }

    public HotkeyMode Mode { get; init; } = HotkeyMode.PushToTalk;
}

/// <summary>رویداد فعال‌شدن یک کلید میانبر.</summary>
public sealed class HotkeyTriggeredEventArgs(HotkeyBinding binding, bool isPressed) : EventArgs
{
    public HotkeyBinding Binding { get; } = binding;

    /// <summary>در حالت Push-to-Talk: true هنگام فشار، false هنگام رها شدن.</summary>
    public bool IsPressed { get; } = isPressed;
}

/// <summary>
/// شنود کلیدهای میانبر سراسری (low-level keyboard hook / RegisterHotKey).
/// پیاده‌سازی در Milestone M5 (Pva.Hotkeys).
/// </summary>
public interface IHotkeyService : IDisposable
{
    event EventHandler<HotkeyTriggeredEventArgs>? Triggered;

    void Register(HotkeyBinding binding);

    void UnregisterAll();
}
