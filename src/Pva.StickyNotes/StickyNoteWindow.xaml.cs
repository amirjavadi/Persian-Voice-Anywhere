using System.Windows;
using System.Windows.Input;

namespace Pva.StickyNotes;

/// <summary>پنجره‌ی یک یادداشت چسبان: بدون‌قاب، قابل جابجایی، pin‌شونده.</summary>
public partial class StickyNoteWindow : Window
{
    private readonly StickyNoteViewModel _viewModel;
    private readonly Action<StickyNoteViewModel> _onDelete;

    public StickyNoteWindow(StickyNoteViewModel viewModel, Action<StickyNoteViewModel> onDelete)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _onDelete = onDelete;
        DataContext = viewModel;
    }

    private void OnDragWindow(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        _onDelete(_viewModel);
        Close();
    }
}
