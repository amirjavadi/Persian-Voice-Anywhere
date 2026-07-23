using System.Runtime.InteropServices;
using Pva.Core;

namespace Pva.Injection;

/// <summary>
/// تزریق متن و کنش‌ها در اپ فوکوس‌دار با <b>Windows SendInput</b> (Unicode). بدون
/// Copy/Paste؛ دقیقاً مثل کیبورد واقعی تایپ می‌کند. جفت‌های surrogate (کاراکترهای فراتر
/// از BMP) به‌صورت دو واحد UTF-16 ارسال می‌شوند که SendInput آن‌ها را درست ترکیب می‌کند.
///
/// محدودیت (ریسک R3): اگر اپ مقصد با دسترسی Administrator اجرا شده باشد و برنامه‌ی ما
/// نه، ویندوز به‌خاطر UIPI ورودی را رد می‌کند. راه‌حل: اجرای برنامه با دسترسی بالا.
/// </summary>
public sealed class SendInputTextInjector : ITextInjector
{
    private readonly int _charDelayMs;

    public SendInputTextInjector(int charDelayMs = 0) => _charDelayMs = charDelayMs;

    public async Task TypeAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        foreach (var ch in text)
        {
            ct.ThrowIfCancellationRequested();
            Send([NativeMethods.UnicodeDown(ch), NativeMethods.UnicodeUp(ch)]);

            if (_charDelayMs > 0)
            {
                await Task.Delay(_charDelayMs, ct);
            }
        }
    }

    public Task SendActionAsync(EditorAction action, CancellationToken ct = default)
    {
        foreach (var chord in EditorActionMapper.Map(action))
        {
            ct.ThrowIfCancellationRequested();
            SendChord(chord);
        }

        return Task.CompletedTask;
    }

    private static void SendChord(KeyChord chord)
    {
        var inputs = new List<NativeMethods.INPUT>(6);

        if (chord.Ctrl)
        {
            inputs.Add(NativeMethods.KeyDown(NativeMethods.VkControl));
        }

        if (chord.Shift)
        {
            inputs.Add(NativeMethods.KeyDown(NativeMethods.VkShift));
        }

        inputs.Add(NativeMethods.KeyDown(chord.VirtualKey));
        inputs.Add(NativeMethods.KeyUp(chord.VirtualKey));

        if (chord.Shift)
        {
            inputs.Add(NativeMethods.KeyUp(NativeMethods.VkShift));
        }

        if (chord.Ctrl)
        {
            inputs.Add(NativeMethods.KeyUp(NativeMethods.VkControl));
        }

        Send(inputs.ToArray());
    }

    private static void Send(NativeMethods.INPUT[] inputs)
    {
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            // شکستِ بی‌صدا ممنوع: خطا به orchestrator → ProcessingFailed → نمایش به کاربر می‌رسد.
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"تزریق متن ناموفق بود (SendInput {sent}/{inputs.Length}، خطای ویندوز {error}). " +
                "اگر برنامه‌ی مقصد با دسترسی Administrator اجرا شده، این برنامه را هم با همان دسترسی اجرا کنید.");
        }
    }
}
