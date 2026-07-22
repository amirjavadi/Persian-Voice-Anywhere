namespace Pva.TextExpansion;

/// <summary>میان‌بُرهای پیش‌فرض (نمونه). کاربر می‌تواند در text-expansions.json تغییر دهد.</summary>
public static class TextExpansionDefaults
{
    public static readonly IReadOnlyDictionary<string, string> Expansions = new Dictionary<string, string>
    {
        ["/email"] = "example@email.com",
        ["/phone"] = "۰۹۱۲۰۰۰۰۰۰۰",
        ["/addr"] = "تهران، ...",
        ["امضا"] = "با احترام،\nامیر جوادی",
    };
}
