using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Pva.App.ViewModels;

namespace Pva.App.Views;

/// <summary>پنجره‌ی میکروفون شناور: بدون‌قاب، Always-On-Top، قابل جابجایی، با انیمیشن شنیدن.</summary>
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
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
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

        // احترام به «کاهش حرکت» سیستم: انیمیشن فقط وقتی مجاز و در حال شنیدن است.
        if (listening && SystemParameters.ClientAreaAnimation)
        {
            breathe.Begin(this, isControllable: true);
            halo.Begin(this, isControllable: true);
        }
        else
        {
            breathe.Stop(this);
            halo.Stop(this);
        }
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
