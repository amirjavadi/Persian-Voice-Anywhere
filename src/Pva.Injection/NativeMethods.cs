using System.Runtime.InteropServices;

namespace Pva.Injection;

/// <summary>P/Invoke برای Windows SendInput (تزریق کلید و کاراکتر Unicode).</summary>
internal static partial class NativeMethods
{
    public const uint InputKeyboard = 1;
    public const uint KeyEventKeyUp = 0x0002;
    public const uint KeyEventUnicode = 0x0004;

    public const ushort VkControl = 0x11;
    public const ushort VkShift = 0x10;

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint Type;
        public InputUnion U;
    }

    /// <summary>
    /// union واقعی ویندوز شامل MOUSEINPUT/KEYBDINPUT/HARDWAREINPUT است و اندازه‌اش را
    /// بزرگ‌ترین عضو (MOUSEINPUT، ۳۲ بایت روی x64) تعیین می‌کند. بدون MOUSEINPUT، اندازه‌ی
    /// INPUT به‌جای ۴۰ بایت ۳۲ بایت می‌شد و SendInput همه‌ی ورودی‌ها را با خطای ۸۷
    /// (ERROR_INVALID_PARAMETER) بی‌صدا رد می‌کرد — یعنی هیچ متنی هرگز تایپ نمی‌شد.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mi;

        [FieldOffset(0)]
        public KEYBDINPUT Ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public nint DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort WVk;
        public ushort WScan;
        public uint DwFlags;
        public uint Time;
        public nint DwExtraInfo;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial uint SendInput(uint nInputs, [In] INPUT[] pInputs, int cbSize);

    public static INPUT KeyDown(ushort vk) => KeyboardInput(vk, 0, 0);

    public static INPUT KeyUp(ushort vk) => KeyboardInput(vk, 0, KeyEventKeyUp);

    public static INPUT UnicodeDown(char ch) => KeyboardInput(0, ch, KeyEventUnicode);

    public static INPUT UnicodeUp(char ch) => KeyboardInput(0, ch, KeyEventUnicode | KeyEventKeyUp);

    private static INPUT KeyboardInput(ushort vk, ushort scan, uint flags) => new()
    {
        Type = InputKeyboard,
        U = new InputUnion
        {
            Ki = new KEYBDINPUT
            {
                WVk = vk,
                WScan = scan,
                DwFlags = flags,
                Time = 0,
                DwExtraInfo = nint.Zero,
            },
        },
    };
}
