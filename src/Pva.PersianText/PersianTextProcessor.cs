using System.Text;
using System.Text.RegularExpressions;
using Pva.Core;

namespace Pva.PersianText;

/// <summary>
/// پس‌پردازش متن فارسیِ خروجی Whisper. خالص و قطعی (بدون I/O و بدون حالت مشترک) تا با
/// تست‌های golden پوشش داده شود. قوانین به‌ترتیب اعمال می‌شوند:
/// نرمال‌سازی فاصله → نرمال‌سازی حروف → جایگزینی اصطلاحات → نیم‌فاصله (ZWNJ) →
/// علائم نگارشی → ارقام فارسی → جمع‌بندی فاصله‌ها.
///
/// وجه تمایز محصول است؛ ترکیب فارسی/انگلیسی و توکن‌های فنی (مثل GitHub، Pull Request)
/// دست‌نخورده می‌مانند چون قوانین فقط روی محدوده‌ی حروف فارسی عمل می‌کنند.
/// </summary>
public sealed partial class PersianTextProcessor : IPersianTextProcessor
{
    private readonly IReadOnlyDictionary<string, string> _replacements;

    public PersianTextProcessor(IReadOnlyDictionary<string, string>? replacements = null)
        => _replacements = replacements ?? new Dictionary<string, string>();

    public string Process(string raw, PersianTextOptions options)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        ArgumentNullException.ThrowIfNull(options);

        var text = NormalizeWhitespace(raw);

        if (options.NormalizeArabicLetters)
        {
            text = NormalizeLetters(text);
        }

        if (_replacements.Count > 0)
        {
            text = ApplyReplacements(text);
        }

        if (options.ApplyZeroWidthNonJoiner)
        {
            text = ApplyZwnj(text);
        }

        if (options.FixPunctuationSpacing)
        {
            text = FixPunctuation(text);
        }

        if (options.UsePersianDigits)
        {
            text = ToPersianDigits(text);
        }

        return CollapseSpaces(text).Trim();
    }

    // --- نرمال‌سازی فاصله و حروف ---

    private static string NormalizeWhitespace(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var c in s)
        {
            switch (c)
            {
                case 'ـ':
                    continue; // تطویل (کشیدگی) حذف می‌شود
                case '\r':
                    continue; // CR حذف؛ فقط \n نگه داشته می‌شود
                case '\n':
                    sb.Append('\n');
                    continue;
                case PersianChars.Zwnj:
                    sb.Append(PersianChars.Zwnj); // نیم‌فاصله حفظ می‌شود
                    continue;
            }

            // هر فاصله‌ی یونیکدِ دیگر به فاصله‌ی معمولی تبدیل می‌شود.
            sb.Append(char.IsWhiteSpace(c) ? ' ' : c);
        }

        return sb.ToString();
    }

    private static string NormalizeLetters(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var c in s)
        {
            // اعراب/حرکات (U+064B..U+065F و U+0670) حذف می‌شوند.
            if (c is (>= 'ً' and <= 'ٟ') or 'ٰ')
            {
                continue;
            }

            sb.Append(PersianChars.ArabicToPersian.TryGetValue(c, out var mapped) ? mapped : c);
        }

        return sb.ToString();
    }

    private string ApplyReplacements(string s)
    {
        // جایگزینی اصطلاحات محافظت‌شده (مثلاً «گیت هاب» → «GitHub»)، طولانی‌ترین ابتدا.
        foreach (var pair in _replacements.OrderByDescending(p => p.Key.Length))
        {
            s = s.Replace(pair.Key, pair.Value, StringComparison.Ordinal);
        }

        return s;
    }

    // --- نیم‌فاصله ---

    private static string ApplyZwnj(string s)
    {
        // پیشوند می/نمی: «می روم» → «می‌روم»
        s = MiPrefixRegex().Replace(s, "$1$2" + PersianChars.ZwnjString);

        // پسوندهای جمع/تفضیلی: «کتاب ها» → «کتاب‌ها»، «بزرگ تر» → «بزرگ‌تر»
        s = SuffixRegex().Replace(s, PersianChars.ZwnjString + "$1");

        return s;
    }

    // --- علائم نگارشی ---

    private static string FixPunctuation(string s)
    {
        // تبدیل علائم لاتین به فارسی وقتی در بافت فارسی‌اند (بعد از حرف فارسی).
        s = LatinCommaRegex().Replace(s, "،");
        s = LatinSemicolonRegex().Replace(s, "؛");
        s = LatinQuestionRegex().Replace(s, "؟");

        // حذف فاصله‌ی پیش از علائم.
        s = SpaceBeforePunctRegex().Replace(s, "$1");
        // حذف فاصله‌ی پیش از نقطه در بافت فارسی.
        s = SpaceBeforeDotRegex().Replace(s, ".");

        // یک فاصله پس از علائم فارسی در صورت نبود.
        s = SpaceAfterPunctRegex().Replace(s, "$1 ");

        // پرانتزها: «( متن )» → «(متن)»
        s = OpenParenRegex().Replace(s, "(");
        s = CloseParenRegex().Replace(s, ")");

        return s;
    }

    // --- ارقام فارسی ---

    private static string ToPersianDigits(string s)
    {
        return DigitRunRegex().Replace(s, match =>
        {
            var index = match.Index;
            var end = index + match.Length;

            // اگر رقم به یک حرف لاتین چسبیده باشد (نسخه/شناسه مثل v2 یا iOS16)، دست نزن.
            if (index > 0 && PersianChars.IsLatinLetter(s[index - 1]))
            {
                return match.Value;
            }

            if (end < s.Length && PersianChars.IsLatinLetter(s[end]))
            {
                return match.Value;
            }

            var sb = new StringBuilder(match.Length);
            foreach (var c in match.Value)
            {
                var v = PersianChars.DigitValue(c);
                sb.Append(v >= 0 ? PersianChars.PersianDigits[v] : c);
            }

            return sb.ToString();
        });
    }

    private static string CollapseSpaces(string s)
    {
        s = HorizontalSpaceRegex().Replace(s, " ");
        s = SpaceAroundNewlineRegex().Replace(s, "\n");
        return s;
    }

    // --- الگوهای کامپایل‌شده ---

    [GeneratedRegex(@"(^|[\s(«])(نمی|می)[ ]+")]
    private static partial Regex MiPrefixRegex();

    [GeneratedRegex(@"(?<=[؀-ۿ])[ ]+(هایشان|هایتان|هایمان|هایی|هایش|هایت|هایم|های|ها|ترین|تر)(?=[\s.،؛:؟!)»]|$)")]
    private static partial Regex SuffixRegex();

    [GeneratedRegex(@"(?<=[؀-ۿ])\s*,")]
    private static partial Regex LatinCommaRegex();

    [GeneratedRegex(@"(?<=[؀-ۿ])\s*;")]
    private static partial Regex LatinSemicolonRegex();

    [GeneratedRegex(@"(?<=[؀-ۿ])\s*\?")]
    private static partial Regex LatinQuestionRegex();

    [GeneratedRegex(@"\s+([،؛؟!:])")]
    private static partial Regex SpaceBeforePunctRegex();

    [GeneratedRegex(@"(?<=[؀-ۿ])\s+\.")]
    private static partial Regex SpaceBeforeDotRegex();

    [GeneratedRegex(@"([،؛؟!])(?=[^\s)\]»…])")]
    private static partial Regex SpaceAfterPunctRegex();

    [GeneratedRegex(@"\(\s+")]
    private static partial Regex OpenParenRegex();

    [GeneratedRegex(@"\s+\)")]
    private static partial Regex CloseParenRegex();

    [GeneratedRegex(@"[0-9٠-٩۰-۹]+")]
    private static partial Regex DigitRunRegex();

    [GeneratedRegex(@"[ \t\f\v]+")]
    private static partial Regex HorizontalSpaceRegex();

    [GeneratedRegex(@" *\n *")]
    private static partial Regex SpaceAroundNewlineRegex();
}
