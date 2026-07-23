namespace Pva.PersianText;

/// <summary>
/// پرامپت اولیه‌ی پیش‌فرض برای جهت‌دهی تشخیص Whisper به‌سمت فارسیِ درست و حفظ اصطلاحات
/// فنی/انگلیسی. به‌عنوان <c>SttOptions.InitialPrompt</c> استفاده می‌شود.
/// </summary>
public static class PersianInitialPrompt
{
    // نکته: Whisper «سبک» پرامپت را تقلید می‌کند، پس نمونه‌ی خوش‌نگارش (نیم‌فاصله‌ی درست،
    // ترکیب فارسی/انگلیسی، علائم) بسیار مؤثرتر از جمله‌ی دستوری است.
    public const string Default =
        "امروز یک Pull Request روی GitHub زدم و کد JavaScript را در Visual Studio بررسی کردم. " +
        "می‌خواهم با Docker و API کار کنم؛ فایل‌ها را ذخیره می‌کنم و نتیجه‌اش را می‌بینم. " +
        "این متن‌ها با نیم‌فاصله و علائم نگارشی درست نوشته شده‌اند.";
}
