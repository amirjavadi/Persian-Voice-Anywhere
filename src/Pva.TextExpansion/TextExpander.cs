using System.Text;
using Pva.Core;

namespace Pva.TextExpansion;

/// <summary>
/// گسترش <b>خالص</b> متن: میان‌بُرها را با متن کامل جایگزین می‌کند. تطبیق عبارت (تا چند
/// کلمه، طولانی‌ترین ابتدا) با نرمال‌سازی سبک (ی/ک فارسی، حروف کوچک). این هم Text
/// Expansion (مثل «/phone») و هم Voice Macro (مثل «امضا») را پوشش می‌دهد.
/// </summary>
public sealed class TextExpander : ITextExpander
{
    private readonly Dictionary<string, string> _expansions;
    private readonly int _maxWords;

    public TextExpander(IReadOnlyDictionary<string, string>? expansions = null)
    {
        var source = expansions ?? TextExpansionDefaults.Expansions;
        _expansions = new Dictionary<string, string>();
        foreach (var pair in source)
        {
            _expansions[Normalize(pair.Key)] = pair.Value;
        }

        _maxWords = _expansions.Count == 0 ? 1 : _expansions.Keys.Max(k => k.Split(' ').Length);
    }

    public string Expand(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || _expansions.Count == 0)
        {
            return text;
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new StringBuilder();

        var i = 0;
        while (i < words.Length)
        {
            var matched = false;
            var maxLen = Math.Min(_maxWords, words.Length - i);
            for (var len = maxLen; len >= 1; len--)
            {
                var phrase = Normalize(string.Join(' ', words, i, len));
                if (_expansions.TryGetValue(phrase, out var replacement))
                {
                    Append(result, replacement);
                    i += len;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                Append(result, words[i]);
                i++;
            }
        }

        return result.ToString();
    }

    private static void Append(StringBuilder builder, string value)
    {
        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(value);
    }

    private static string Normalize(string s)
        => s.Trim().ToLowerInvariant().Replace('ي', 'ی').Replace('ك', 'ک');
}
