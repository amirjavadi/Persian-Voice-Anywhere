using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Pva.App;

/// <summary>
/// آیکون اختصاصی System Tray را در زمان اجرا می‌سازد: دایره‌ی گرادیانیِ برند
/// (فیروزه‌ای → بنفش، هویت Liquid Glass) با میکروفون سفید. بدون فایل ico خارجی؛
/// کاملاً پرتابل.
/// </summary>
internal static class TrayIconFactory
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(nint hIcon);

    public static Icon Create()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var gradient = new LinearGradientBrush(
                new Rectangle(0, 0, 32, 32),
                Color.FromArgb(0x13, 0xB9, 0xAC),
                Color.FromArgb(0x6D, 0x5E, 0xF6),
                45f);
            g.FillEllipse(gradient, 1, 1, 30, 30);

            // بدنه‌ی میکروفون (کپسول سفید).
            using var body = new SolidBrush(Color.White);
            using var bodyPath = RoundedRect(new RectangleF(13f, 7f, 6f, 11f), 3f);
            g.FillPath(body, bodyPath);

            // کمان پایه + پایه‌ی میکروفون.
            using var pen = new Pen(Color.White, 2.2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };
            g.DrawArc(pen, 10.5f, 10f, 11f, 10f, 0f, 180f);
            g.DrawLine(pen, 16f, 20f, 16f, 24f);
        }

        // GetHicon یک handle بومی می‌دهد؛ clone مدیریت‌شده می‌گیریم و handle را آزاد می‌کنیم.
        var hIcon = bitmap.GetHicon();
        try
        {
            using var native = Icon.FromHandle(hIcon);
            return (Icon)native.Clone();
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static GraphicsPath RoundedRect(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2f;
        path.AddArc(rect.X, rect.Y, d, d, 180f, 90f);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270f, 90f);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0f, 90f);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90f, 90f);
        path.CloseFigure();
        return path;
    }
}
