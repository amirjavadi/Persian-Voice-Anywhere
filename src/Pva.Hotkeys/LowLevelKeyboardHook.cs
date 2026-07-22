using System.Runtime.InteropServices;

namespace Pva.Hotkeys;

/// <summary>
/// نصب یک low-level keyboard hook سراسری (WH_KEYBOARD_LL). callback باید روی نخی با
/// message loop اجرا شود، پس یک نخ اختصاصی با حلقه‌ی پیام راه می‌اندازد. رویداد
/// <see cref="KeyEvent"/> با (vkCode، isDown) صادر می‌شود.
///
/// نیاز به اجرای واقعی روی ویندوز دارد؛ تأیید نهایی دستی است.
/// </summary>
internal sealed class LowLevelKeyboardHook : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;
    private const uint WmQuit = 0x0012;

    private readonly ManualResetEventSlim _ready = new(false);
    private HookProc? _proc;
    private nint _hookId;
    private uint _threadId;
    private Thread? _thread;

    public event Action<int, bool>? KeyEvent;

    public void Start()
    {
        if (_thread is not null)
        {
            return;
        }

        _thread = new Thread(Run) { IsBackground = true, Name = "pva-kbd-hook" };
        _thread.Start();
        _ready.Wait();
    }

    private void Run()
    {
        _proc = HookCallback;
        _hookId = SetWindowsHookEx(WhKeyboardLl, _proc, GetModuleHandle(null), 0);
        _threadId = GetCurrentThreadId();
        _ready.Set();

        while (GetMessage(out var msg, nint.Zero, 0, 0) > 0)
        {
            // فقط برای زنده نگه‌داشتن حلقه‌ی پیام؛ callback خودش صدا زده می‌شود.
            _ = msg;
        }
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            var message = (int)wParam;
            var vkCode = Marshal.ReadInt32(lParam); // اولین فیلد KBDLLHOOKSTRUCT
            var isDown = message is WmKeyDown or WmSysKeyDown;
            var isUp = message is WmKeyUp or WmSysKeyUp;

            if (isDown || isUp)
            {
                KeyEvent?.Invoke(vkCode, isDown);
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hookId != nint.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = nint.Zero;
        }

        if (_threadId != 0)
        {
            PostThreadMessage(_threadId, WmQuit, nint.Zero, nint.Zero);
            _threadId = 0;
        }

        _ready.Dispose();
    }

    private delegate nint HookProc(int nCode, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public nint Hwnd;
        public uint Message;
        public nint WParam;
        public nint LParam;
        public uint Time;
        public int PtX;
        public int PtY;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern int GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostThreadMessage(uint idThread, uint msg, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint GetModuleHandle(string? lpModuleName);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();
}
