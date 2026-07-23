using System.ComponentModel;
using System.Windows;

namespace Pva.Notepad;

/// <summary>پنجره‌ی نوت‌پد تب‌دار. session را به‌صورت خودکار و هنگام بسته‌شدن ذخیره می‌کند.</summary>
public partial class NotepadWindow : Window
{
    private readonly NotepadViewModel _viewModel;

    public NotepadWindow(NotepadViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // تأییدِ دور انداختن محتوای ذخیره‌نشده‌ی یک فایل، کارِ لایه‌ی View است تا ViewModel
        // تست‌پذیر بماند (در تست، ConfirmDiscard = null یعنی بدون مانع).
        _viewModel.ConfirmDiscard = document =>
            MessageBox.Show(
                $"«{document.Title}» تغییرات ذخیره‌نشده دارد. بدون ذخیره بسته شود؟",
                "نوت‌پد",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // محتوای همه‌ی تب‌ها در session ذخیره می‌شود؛ فقط برای فایل‌های ذخیره‌نشده هشدار می‌دهیم.
        _viewModel.SaveSession();
        base.OnClosing(e);
    }
}
