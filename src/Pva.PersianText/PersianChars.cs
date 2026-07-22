namespace Pva.PersianText;

/// <summary>ثابت‌ها و نگاشت‌های کاراکتری فارسی.</summary>
internal static class PersianChars
{
    /// <summary>نیم‌فاصله (Zero-Width Non-Joiner).</summary>
    public const char Zwnj = '‌';

    public const string ZwnjString = "‌";

    /// <summary>ارقام فارسی ۰..۹ (U+06F0..U+06F9).</summary>
    public static readonly char[] PersianDigits =
        ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];

    /// <summary>نگاشت نرمال‌سازی حروف عربی به فارسی.</summary>
    public static readonly Dictionary<char, char> ArabicToPersian = new()
    {
        ['ي'] = 'ی', // ي → ی (Arabic Yeh → Farsi Yeh)
        ['ى'] = 'ی', // ى → ی (Alef Maksura → Farsi Yeh)
        ['ك'] = 'ک', // ك → ک (Arabic Kaf → Keheh)
        ['ة'] = 'ه', // ة → ه (Teh Marbuta → Heh)
        ['أ'] = 'ا', // أ → ا
        ['إ'] = 'ا', // إ → ا
        ['ؤ'] = 'و', // ؤ → و
    };

    /// <summary>آیا کاراکتر یک حرف فارسی/عربی است.</summary>
    public static bool IsPersianLetter(char c) => c is >= '؀' and <= 'ۿ';

    /// <summary>آیا کاراکتر یک حرف لاتین است.</summary>
    public static bool IsLatinLetter(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    /// <summary>مقدار عددی یک رقم (ASCII یا عربیِ هندی)، یا -1.</summary>
    public static int DigitValue(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= '٠' and <= '٩' => c - '٠', // ٠..٩ Arabic-Indic
        >= '۰' and <= '۹' => c - '۰', // ۰..۹ Persian (already)
        _ => -1,
    };
}
