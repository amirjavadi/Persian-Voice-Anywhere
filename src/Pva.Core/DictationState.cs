namespace Pva.Core;

/// <summary>
/// وضعیت فعلی خط‌لوله‌ی دیکته که در UI (میکروفون شناور، tray) نمایش داده می‌شود.
/// </summary>
public enum DictationState
{
    /// <summary>بی‌کار؛ منتظر کلید میانبر. مدل ممکن است در حافظه نباشد.</summary>
    Idle,

    /// <summary>در حال ضبط صدای کاربر.</summary>
    Listening,

    /// <summary>در حال رونویسی و پس‌پردازش.</summary>
    Processing,

    /// <summary>در حال تزریق متن به اپ مقصد.</summary>
    Injecting,
}
