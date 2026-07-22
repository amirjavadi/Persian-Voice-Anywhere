namespace Pva.Core;

/// <summary>گزینه‌های تفسیر دستورهای صوتی.</summary>
public sealed record CommandOptions
{
    /// <summary>اگر true، عبارات رزرو («خط بعد»، «ویرگول»…) به کنش تبدیل می‌شوند؛
    /// اگر false، عیناً به‌عنوان متن دیکته می‌شوند (رفع ابهام دستور/دیکته — ریسک R6).</summary>
    public bool CommandModeEnabled { get; init; } = true;
}

/// <summary>یک قطعه از خروجی تفسیرشده: یا متن است یا یک کنش ویرایشی.</summary>
public sealed record TranscriptPart
{
    public string? Text { get; init; }

    public EditorAction? Action { get; init; }

    public static TranscriptPart OfText(string text) => new() { Text = text };

    public static TranscriptPart OfAction(EditorAction action) => new() { Action = action };
}

/// <summary>خروجی تفسیرشده‌ی یک رونویسی: دنباله‌ای از متن‌ها و کنش‌ها به ترتیب.</summary>
public sealed record ParsedTranscript(IReadOnlyList<TranscriptPart> Parts);

/// <summary>
/// تشخیص دستورهای صوتی در متن خام و تبدیل آن‌ها به کنش. پیاده‌سازی در Milestone M6
/// (Pva.Commands).
/// </summary>
public interface ICommandParser
{
    ParsedTranscript Parse(string rawText, CommandOptions options);
}
