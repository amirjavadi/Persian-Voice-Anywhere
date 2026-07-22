using System.Windows;

namespace Pva.Clipboard;

/// <summary>پنجره‌ی تاریخچه‌ی کلیپ‌بورد.</summary>
public partial class ClipboardHistoryWindow : Window
{
    public ClipboardHistoryWindow(ClipboardHistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
