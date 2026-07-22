namespace Pva.Core;

/// <summary>کنش‌های ویرایشی که به‌جای متن، به‌صورت کلید به اپ مقصد ارسال می‌شوند.</summary>
public enum EditorAction
{
    NewLine,
    NewParagraph,
    Backspace,
    DeleteWord,
    Undo,
    Redo,
    Space,
}

/// <summary>
/// تزریق متن و کنش‌ها در اپ فوکوس‌دار با Windows SendInput (Unicode). بدون Copy/Paste.
/// پیاده‌سازی در Milestone M4 (Pva.Injection).
/// </summary>
public interface ITextInjector
{
    Task TypeAsync(string text, CancellationToken ct = default);

    Task SendActionAsync(EditorAction action, CancellationToken ct = default);
}
