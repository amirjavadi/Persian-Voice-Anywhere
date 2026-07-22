using System.ComponentModel;
using System.Windows;

namespace Pva.Notepad;

/// <summary>پنجره‌ی نوت‌پد تب‌دار. هنگام بسته‌شدن، session را ذخیره می‌کند.</summary>
public partial class NotepadWindow : Window
{
    private readonly NotepadViewModel _viewModel;

    public NotepadWindow(NotepadViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _viewModel.SaveSession();
        base.OnClosing(e);
    }
}
