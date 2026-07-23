using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Pva.App.ViewModels;

namespace Pva.App.Views;

/// <summary>پنجره‌ی میکروفون شناور: بدون‌قاب، Always-On-Top، قابل جابجایی، با انیمیشن شنیدن.</summary>
public partial class FloatingMicWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(nint hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    private readonly DictationViewModel _viewModel;

    public FloatingMicWindow(DictationViewModel viewModel, double opacity, bool topmost)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Opacity = opacity;
        Topmost = topmost;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// پنجره را «non-activating» می‌کند: کلیک روی گوی، فوکوس کیبورد را از برنامه‌ی هدف
    /// (Word، Notepad، Chrome…) نمی‌دزدد. بدون این، متنِ رونویسی‌شده به‌جای برنامه‌ی مقصد
    /// به همین پنجره تزریق می‌شد و ظاهراً «هیچ اتفاقی نمی‌افتاد».
    /// </summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        var exStyle = GetWindowLong(handle, GwlExStyle);
        _ = SetWindowLong(handle, GwlExStyle, exStyle | WsExNoActivate | WsExToolWindow);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DictationViewModel.IsListening))
        {
            UpdateAnimations(_viewModel.IsListening);
        }
    }

    private void UpdateAnimations(bool listening)
    {
        var breathe = (Storyboard)FindResource("BreatheStoryboard");
        var halo = (Storyboard)FindResource("HaloStoryboard");
        var eq = (Storyboard)FindResource("EqStoryboard");

        // رنگ نقطه‌ی وضعیت: فیروزه‌ی برند هنگام شنیدن، خاکستری در Idle.
        StateDot.Fill = listening
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x13, 0xB9, 0xAC))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6B, 0x70, 0x80));
        Equalizer.Visibility = listening ? Visibility.Visible : Visibility.Collapsed;

        // احترام به «کاهش حرکت» سیستم: انیمیشن فقط وقتی مجاز و در حال شنیدن است.
        if (listening && SystemParameters.ClientAreaAnimation)
        {
            breathe.Begin(this, isControllable: true);
            halo.Begin(this, isControllable: true);
            eq.Begin(this, isControllable: true);
        }
        else
        {
            breathe.Stop(this);
            halo.Stop(this);
            eq.Stop(this);
        }
    }

    // جدا کردن «کشیدن پنجره» از «کلیک روی گوی». فراخوانی مستقیم DragMove روی MouseDown
    // یک حلقه‌ی پیام تودرفتو اجرا می‌کرد و رویداد MouseUp گوی (که ضبط را toggle می‌کند)
    // را می‌بلعید؛ در نتیجه کلیک روی میکروفون هیچ‌گاه ضبط را شروع نمی‌کرد. حالا فقط پس از
    // عبور از آستانه‌ی حرکت، کشیدن آغاز می‌شود و کلیک ساده به‌درستی toggle می‌کند.
    private Point? _pressOrigin;
    private bool _dragged;

    private void OnRootMouseDown(object sender, MouseButtonEventArgs e)
    {
        _pressOrigin = e.GetPosition(this);
        _dragged = false;
    }

    private void OnRootMouseMove(object sender, MouseEventArgs e)
    {
        if (_pressOrigin is not { } origin || e.LeftButton != MouseButtonState.Pressed || _dragged)
        {
            return;
        }

        var current = e.GetPosition(this);
        if (Math.Abs(current.X - origin.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(current.Y - origin.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _dragged = true;
        DragMove(); // تا رها شدن دکمه بلاک می‌شود؛ پس از بازگشت، کلیک گوی رخ نمی‌دهد.
        _pressOrigin = null;
    }

    private void OnRootMouseUp(object sender, MouseButtonEventArgs e) => _pressOrigin = null;

    private void OnOrbClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        _pressOrigin = null;
        if (_dragged)
        {
            _dragged = false;
            return; // این یک کشیدن بود، نه کلیک.
        }

        if (_viewModel.ToggleCommand.CanExecute(null))
        {
            _viewModel.ToggleCommand.Execute(null);
        }
    }
}
