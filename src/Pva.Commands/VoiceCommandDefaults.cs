using Pva.Core;

namespace Pva.Commands;

/// <summary>
/// دستورهای صوتی پیش‌فرض فارسی. کلیدها به‌صورت نرمال‌شده (ی/ک فارسی، حروف کوچک) هستند
/// چون parser عبارت را قبل از جست‌وجو نرمال می‌کند.
/// </summary>
public static class VoiceCommandDefaults
{
    /// <summary>عبارت → کنش ویرایشی.</summary>
    public static readonly IReadOnlyDictionary<string, EditorAction> Actions = new Dictionary<string, EditorAction>
    {
        ["خط بعد"] = EditorAction.NewLine,
        ["خط جدید"] = EditorAction.NewLine,
        ["برو خط بعد"] = EditorAction.NewLine,
        ["اینتر"] = EditorAction.NewLine,
        ["پاراگراف جدید"] = EditorAction.NewParagraph,
        ["پاراگراف بعد"] = EditorAction.NewParagraph,
        ["حذف کلمه قبل"] = EditorAction.DeleteWord,
        ["حذف کلمه"] = EditorAction.DeleteWord,
        ["پاک کن"] = EditorAction.DeleteWord,
        ["بازگردانی"] = EditorAction.Undo,
        ["undo"] = EditorAction.Undo,
        ["آندو"] = EditorAction.Undo,
        ["بازانجام"] = EditorAction.Redo,
        ["redo"] = EditorAction.Redo,
        ["ریدو"] = EditorAction.Redo,
        ["فاصله"] = EditorAction.Space,
    };

    /// <summary>عبارت → علامت نگارشی (درون متن درج می‌شود).</summary>
    public static readonly IReadOnlyDictionary<string, string> Punctuation = new Dictionary<string, string>
    {
        ["ویرگول"] = "،",
        ["نقطه"] = ".",
        ["علامت سوال"] = "؟",
        ["علامت سؤال"] = "؟",
        ["علامت تعجب"] = "!",
        ["دو نقطه"] = ":",
        ["نقطه ویرگول"] = "؛",
        ["پرانتز باز"] = "(",
        ["پرانتز بسته"] = ")",
        ["گیومه باز"] = "«",
        ["گیومه بسته"] = "»",
    };
}
