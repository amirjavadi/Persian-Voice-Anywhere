namespace Pva.Storage;

/// <summary>
/// تنظیمات کاربر که کنار فایل اجرایی در settings.json ذخیره می‌شوند (پرتابل، بدون رجیستری).
/// </summary>
public sealed record AppSettings
{
    /// <summary>ژست کلید میانبر، مثل "Ctrl+Space"، "CapsLock"، "DoubleCtrl".</summary>
    public string HotkeyGesture { get; init; } = "Ctrl+Space";

    /// <summary>"PushToTalk" یا "Toggle".</summary>
    public string HotkeyMode { get; init; } = "PushToTalk";

    /// <summary>"WhisperCpp" یا "FasterWhisper".</summary>
    public string PreferredEngine { get; init; } = "WhisperCpp";

    /// <summary>"Auto" / "Cpu" / "Gpu".</summary>
    public string Device { get; init; } = "Auto";

    /// <summary>"System" / "Light" / "Dark".</summary>
    public string Theme { get; init; } = "System";

    public double MicOpacity { get; init; } = 0.95;

    public bool MicAlwaysOnTop { get; init; } = true;

    public bool UsePersianDigits { get; init; } = true;

    /// <summary>اصلاح هوشمند فارسی (نیم‌فاصله، علائم…).</summary>
    public bool SmartCorrection { get; init; } = true;

    public bool CommandModeEnabled { get; init; } = true;

    public string Language { get; init; } = "fa";
}
