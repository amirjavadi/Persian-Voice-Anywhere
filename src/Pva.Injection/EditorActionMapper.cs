using Pva.Core;

namespace Pva.Injection;

/// <summary>یک ترکیب کلید مجازی ویندوز (به‌همراه مادیفایرها).</summary>
public readonly record struct KeyChord(ushort VirtualKey, bool Ctrl = false, bool Shift = false);

/// <summary>
/// نگاشت <b>خالص</b> از کنش‌های ویرایشی به دنباله‌ی کلیدهای مجازی ویندوز. جدا از تزریق
/// واقعی (SendInput) تا بدون P/Invoke قابل تست باشد.
/// </summary>
public static class EditorActionMapper
{
    // کدهای کلید مجازی ویندوز (Virtual-Key Codes).
    public const ushort VkBack = 0x08;
    public const ushort VkReturn = 0x0D;
    public const ushort VkSpace = 0x20;
    public const ushort VkZ = 0x5A;
    public const ushort VkY = 0x59;

    public static IReadOnlyList<KeyChord> Map(EditorAction action) => action switch
    {
        EditorAction.NewLine => [new KeyChord(VkReturn)],
        EditorAction.NewParagraph => [new KeyChord(VkReturn), new KeyChord(VkReturn)],
        EditorAction.Backspace => [new KeyChord(VkBack)],
        EditorAction.DeleteWord => [new KeyChord(VkBack, Ctrl: true)],
        EditorAction.Undo => [new KeyChord(VkZ, Ctrl: true)],
        EditorAction.Redo => [new KeyChord(VkY, Ctrl: true)],
        EditorAction.Space => [new KeyChord(VkSpace)],
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "کنش ویرایشی پشتیبانی‌نشده."),
    };
}
