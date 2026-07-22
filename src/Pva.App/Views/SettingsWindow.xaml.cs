using System.Windows;
using Pva.App.ViewModels;

namespace Pva.App.Views;

/// <summary>پنجره‌ی تنظیمات.</summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
