namespace Pva.Core;

/// <summary>گزینه‌های پس‌پردازش متن فارسی.</summary>
public sealed record PersianTextOptions
{
    /// <summary>نرمال‌سازی حروف عربی به فارسی (ي/ك ← ی/ک).</summary>
    public bool NormalizeArabicLetters { get; init; } = true;

    /// <summary>درج نیم‌فاصله (ZWNJ) در جایگاه‌های درست.</summary>
    public bool ApplyZeroWidthNonJoiner { get; init; } = true;

    /// <summary>تبدیل اعداد به ارقام فارسی.</summary>
    public bool UsePersianDigits { get; init; } = true;

    /// <summary>اصلاح فاصله‌گذاری علائم نگارشی.</summary>
    public bool FixPunctuationSpacing { get; init; } = true;

    /// <summary>حفظ توکن‌های انگلیسی/کد و اصطلاحات فنی محافظت‌شده.</summary>
    public bool PreserveMixedScript { get; init; } = true;
}

/// <summary>
/// پس‌پردازش متن فارسیِ خروجی Whisper. خالص و قطعی (بدون I/O و بدون حالت مشترک) تا
/// با تست‌های golden-file پوشش داده شود. پیاده‌سازی در Milestone M3 (Pva.PersianText).
/// </summary>
public interface IPersianTextProcessor
{
    string Process(string raw, PersianTextOptions options);
}
