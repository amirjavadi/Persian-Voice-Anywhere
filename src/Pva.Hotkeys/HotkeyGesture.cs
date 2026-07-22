namespace Pva.Hotkeys;

/// <summary>نوع ژست کلید میانبر.</summary>
public enum HotkeyGestureKind
{
    /// <summary>ترکیب مادیفایر(ها) + یک کلید (مثل Ctrl+Space).</summary>
    Combo,

    /// <summary>یک کلید تنها (مثل CapsLock).</summary>
    SingleKey,

    /// <summary>دو بار فشار سریع یک کلید (مثل Double-Ctrl).</summary>
    DoubleTap,
}

/// <summary>
/// نمایش <b>خالص</b> و parse یک ژست کلید میانبر. جدا از hook ویندوز تا قابل تست باشد.
/// نمونه‌ها: "Ctrl+Space"، "Ctrl+Shift+Space"، "CapsLock"، "DoubleCtrl".
/// </summary>
public sealed record HotkeyGesture(
    HotkeyGestureKind Kind,
    ushort Key,
    bool Ctrl = false,
    bool Shift = false,
    bool Alt = false)
{
    public const ushort VkControl = 0x11;
    public const ushort VkShift = 0x10;
    public const ushort VkMenu = 0x12; // Alt
    public const ushort VkSpace = 0x20;
    public const ushort VkCapsLock = 0x14;
    public const ushort VkReturn = 0x0D;

    public static HotkeyGesture Parse(string gesture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gesture);
        var text = gesture.Trim();

        switch (text.Replace(" ", string.Empty).ToLowerInvariant())
        {
            case "doublectrl":
            case "doublecontrol":
                return new HotkeyGesture(HotkeyGestureKind.DoubleTap, VkControl);
            case "capslock":
                return new HotkeyGesture(HotkeyGestureKind.SingleKey, VkCapsLock);
        }

        bool ctrl = false, shift = false, alt = false;
        ushort key = 0;

        foreach (var raw in text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (raw.ToLowerInvariant())
            {
                case "ctrl" or "control":
                    ctrl = true;
                    break;
                case "shift":
                    shift = true;
                    break;
                case "alt":
                    alt = true;
                    break;
                default:
                    key = MapKey(raw);
                    break;
            }
        }

        if (key == 0)
        {
            throw new FormatException($"ژست کلید نامعتبر: '{gesture}'.");
        }

        var kind = ctrl || shift || alt ? HotkeyGestureKind.Combo : HotkeyGestureKind.SingleKey;
        return new HotkeyGesture(kind, key, ctrl, shift, alt);
    }

    private static ushort MapKey(string token)
    {
        if (token.Length == 1)
        {
            var c = char.ToUpperInvariant(token[0]);
            if (c is >= 'A' and <= 'Z')
            {
                return c;
            }

            if (c is >= '0' and <= '9')
            {
                return c;
            }
        }

        return token.ToLowerInvariant() switch
        {
            "space" => VkSpace,
            "enter" or "return" => VkReturn,
            "capslock" => VkCapsLock,
            _ => throw new FormatException($"کلید ناشناخته: '{token}'."),
        };
    }
}
