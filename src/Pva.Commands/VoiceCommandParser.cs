using System.Text;
using Pva.Core;

namespace Pva.Commands;

/// <summary>
/// تفسیر <b>خالص</b> دستورهای صوتی در متن خام Whisper. عبارات رزرو («خط بعد»، «ویرگول»،
/// «حذف کلمه قبل»…) به کنش ویرایشی یا علامت نگارشی تبدیل می‌شوند، نه اینکه تایپ شوند.
/// علائم نگارشی درون همان متن درج می‌شوند تا پس‌پردازش فارسی فاصله‌گذاری را درست کند؛
/// کنش‌های ویرایشی (Enter، Backspace، Undo…) متن را به بخش‌های جدا می‌شکنند.
///
/// رفع ابهام دستور/دیکته (ریسک R6): با <see cref="CommandOptions.CommandModeEnabled"/>
/// می‌توان کل تفسیر را خاموش کرد تا همه‌چیز عیناً دیکته شود.
/// </summary>
public sealed class VoiceCommandParser : ICommandParser
{
    private const int MaxPhraseWords = 3;

    private readonly IReadOnlyDictionary<string, EditorAction> _actions;
    private readonly IReadOnlyDictionary<string, string> _punctuation;

    public VoiceCommandParser(
        IReadOnlyDictionary<string, EditorAction>? actions = null,
        IReadOnlyDictionary<string, string>? punctuation = null)
    {
        _actions = actions ?? VoiceCommandDefaults.Actions;
        _punctuation = punctuation ?? VoiceCommandDefaults.Punctuation;
    }

    public ParsedTranscript Parse(string rawText, CommandOptions options)
    {
        var text = CollapseSpaces(rawText);
        if (text.Length == 0)
        {
            return new ParsedTranscript([]);
        }

        if (!options.CommandModeEnabled)
        {
            return new ParsedTranscript([TranscriptPart.OfText(text)]);
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parts = new List<TranscriptPart>();
        var buffer = new StringBuilder();

        var i = 0;
        while (i < words.Length)
        {
            if (TryMatchCommand(words, i, out var consumed, out var action, out var punct))
            {
                if (action is { } editorAction)
                {
                    FlushBuffer(buffer, parts);
                    parts.Add(TranscriptPart.OfAction(editorAction));
                }
                else if (punct is { } punctuation)
                {
                    buffer.Append(punctuation); // چسبیده به متن، بدون فاصله
                }

                i += consumed;
            }
            else
            {
                AppendWord(buffer, words[i]);
                i++;
            }
        }

        FlushBuffer(buffer, parts);
        return new ParsedTranscript(parts);
    }

    private bool TryMatchCommand(string[] words, int start, out int consumed, out EditorAction? action, out string? punct)
    {
        var maxLen = Math.Min(MaxPhraseWords, words.Length - start);
        for (var len = maxLen; len >= 1; len--)
        {
            var phrase = Normalize(string.Join(' ', words, start, len));

            if (_actions.TryGetValue(phrase, out var mappedAction))
            {
                consumed = len;
                action = mappedAction;
                punct = null;
                return true;
            }

            if (_punctuation.TryGetValue(phrase, out var mappedPunct))
            {
                consumed = len;
                action = null;
                punct = mappedPunct;
                return true;
            }
        }

        consumed = 0;
        action = null;
        punct = null;
        return false;
    }

    private static void AppendWord(StringBuilder buffer, string word)
    {
        // پس از علائم بازکننده «(» و «»» فاصله نمی‌گذاریم تا «(متن)» تمیز بماند.
        if (buffer.Length > 0 && buffer[^1] is not ('(' or '«' or '['))
        {
            buffer.Append(' ');
        }

        buffer.Append(word);
    }

    private static void FlushBuffer(StringBuilder buffer, List<TranscriptPart> parts)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        parts.Add(TranscriptPart.OfText(buffer.ToString()));
        buffer.Clear();
    }

    private static string Normalize(string phrase)
        => phrase.Trim().ToLowerInvariant().Replace('ي', 'ی').Replace('ك', 'ک');

    private static string CollapseSpaces(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return string.Empty;
        }

        return string.Join(' ', s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
