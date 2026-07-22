using System.Windows;
using System.Windows.Input;
using Pva.App.ViewModels;

namespace Pva.App.Views;

/// <summary>پنجره‌ی میکروفون شناور: بدون‌قاب، Always-On-Top، قابل جابجایی، شفافیت قابل‌تنظیم.</summary>
public partial class FloatingMicWindow : Window
{
    private readonly DictationViewModel _viewModel;

    public FloatingMicWindow(DictationViewModel viewModel, double opacity, bool topmost)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Opacity = opacity;
        Topmost = topmost;
    }

    private void OnDragWindow(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OnOrbClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (_viewModel.ToggleCommand.CanExecute(null))
        {
            _viewModel.ToggleCommand.Execute(null);
        }
    }
}
