namespace Pva.Core;

/// <summary>نگاشت وضعیت دیکته به متن فارسیِ نمایش در UI (میکروفون شناور، tray).</summary>
public static class DictationStateText
{
    public static string ToPersian(DictationState state) => state switch
    {
        DictationState.Idle => "آماده",
        DictationState.Listening => "در حال شنیدن…",
        DictationState.Processing => "در حال پردازش…",
        DictationState.Injecting => "در حال تایپ…",
        _ => "نامشخص",
    };
}
