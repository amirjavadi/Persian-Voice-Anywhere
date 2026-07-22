namespace Pva.PersianText;

/// <summary>
/// پرامپت اولیه‌ی پیش‌فرض برای جهت‌دهی تشخیص Whisper به‌سمت فارسیِ درست و حفظ اصطلاحات
/// فنی/انگلیسی. به‌عنوان <c>SttOptions.InitialPrompt</c> استفاده می‌شود.
/// </summary>
public static class PersianInitialPrompt
{
    public const string Default =
        "متن زیر به زبان فارسی است و ممکن است شامل اصطلاحات فنی و نام‌های انگلیسی باشد، " +
        "مانند GitHub، Pull Request، JavaScript، Docker، Visual Studio و API. " +
        "نگارش درست فارسی با نیم‌فاصله و علائم نگارشی رعایت شود.";
}
