namespace Pva.Core;

/// <summary>
/// گسترش متن: جایگزینی میان‌بُرها (مثل «/phone» یا «امضا») با متن کامل. در خط‌لوله‌ی
/// دیکته پس از پس‌پردازش فارسی اعمال می‌شود. پیاده‌سازی در Pva.TextExpansion.
/// </summary>
public interface ITextExpander
{
    string Expand(string text);
}
