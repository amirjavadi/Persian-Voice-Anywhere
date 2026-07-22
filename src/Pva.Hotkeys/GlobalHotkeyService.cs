using Pva.Core;

namespace Pva.Hotkeys;

/// <summary>
/// شنود کلیدهای میانبر سراسری با low-level keyboard hook و تطبیق ژست‌ها. حالت‌های
/// Push-to-Talk، Toggle، Combo، Single-Key و Double-Tap را پشتیبانی می‌کند.
/// نیاز به اجرای واقعی روی ویندوز دارد؛ تأیید نهایی دستی است (ژست‌پارسر جدا تست شده).
/// </summary>
public sealed class GlobalHotkeyService : IHotkeyService
{
    private const int DoubleTapWindowMs = 400;

    private readonly LowLevelKeyboardHook _hook = new();
    private readonly List<Registration> _registrations = new();
    private readonly HashSet<ushort> _pressed = new();
    private readonly Lock _sync = new();
    private bool _started;

    public event EventHandler<HotkeyTriggeredEventArgs>? Triggered;

    public void Register(HotkeyBinding binding)
    {
        var gesture = HotkeyGesture.Parse(binding.Gesture);

        lock (_sync)
        {
            _registrations.Add(new Registration(binding, gesture));
            if (!_started)
            {
                _hook.KeyEvent += OnKeyEvent;
                _hook.Start();
                _started = true;
            }
        }
    }

    public void UnregisterAll()
    {
        lock (_sync)
        {
            _registrations.Clear();
            _pressed.Clear();
        }
    }

    public void Dispose()
    {
        _hook.KeyEvent -= OnKeyEvent;
        _hook.Dispose();
    }

    private void OnKeyEvent(int vkCode, bool isDown)
    {
        var vk = Normalize((ushort)vkCode);

        List<(HotkeyBinding Binding, bool IsPressed)> toRaise = new();
        lock (_sync)
        {
            if (isDown)
            {
                _pressed.Add(vk);
            }
            else
            {
                _pressed.Remove(vk);
            }

            foreach (var reg in _registrations)
            {
                if (Evaluate(reg, vk, isDown, out var pressed))
                {
                    toRaise.Add((reg.Binding, pressed));
                }
            }
        }

        foreach (var (binding, pressed) in toRaise)
        {
            Triggered?.Invoke(this, new HotkeyTriggeredEventArgs(binding, pressed));
        }
    }

    private bool Evaluate(Registration reg, ushort vk, bool isDown, out bool pressed)
    {
        pressed = false;
        var g = reg.Gesture;

        if (g.Kind == HotkeyGestureKind.DoubleTap)
        {
            if (isDown && vk == g.Key)
            {
                var now = Environment.TickCount64;
                var isDouble = now - reg.LastTapMs <= DoubleTapWindowMs;
                reg.LastTapMs = now;
                if (isDouble)
                {
                    reg.LastTapMs = 0;
                    pressed = reg.Toggle = !reg.Toggle;
                    return true;
                }
            }

            return false;
        }

        if (vk != g.Key)
        {
            return false;
        }

        var modsMatch = ModifiersMatch(g);

        if (isDown)
        {
            if (reg.Active || !modsMatch)
            {
                return false; // جلوگیری از تکرار خودکار یا مادیفایر ناقص
            }

            reg.Active = true;
            pressed = reg.Binding.Mode == HotkeyMode.Toggle ? reg.Toggle = !reg.Toggle : true;
            return true;
        }

        // key up
        if (!reg.Active)
        {
            return false;
        }

        reg.Active = false;
        if (reg.Binding.Mode == HotkeyMode.PushToTalk)
        {
            pressed = false;
            return true; // پایان Push-to-Talk
        }

        return false; // Toggle فقط روی down عمل می‌کند
    }

    private bool ModifiersMatch(HotkeyGesture g)
        => g.Ctrl == _pressed.Contains(HotkeyGesture.VkControl)
           && g.Shift == _pressed.Contains(HotkeyGesture.VkShift)
           && g.Alt == _pressed.Contains(HotkeyGesture.VkMenu);

    private static ushort Normalize(ushort vk) => vk switch
    {
        0xA2 or 0xA3 => HotkeyGesture.VkControl, // L/R Control
        0xA0 or 0xA1 => HotkeyGesture.VkShift,   // L/R Shift
        0xA4 or 0xA5 => HotkeyGesture.VkMenu,    // L/R Alt
        _ => vk,
    };

    private sealed class Registration(HotkeyBinding binding, HotkeyGesture gesture)
    {
        public HotkeyBinding Binding { get; } = binding;

        public HotkeyGesture Gesture { get; } = gesture;

        public bool Active { get; set; }

        public bool Toggle { get; set; }

        public long LastTapMs { get; set; }
    }
}
