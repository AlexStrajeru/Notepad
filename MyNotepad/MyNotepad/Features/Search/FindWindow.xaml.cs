using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ICSharpCode.AvalonEdit;
using MyNotepad.Core;

namespace MyNotepad.Features.Search;

// Mosteneste AnimatedWindow — fade-in si dark title bar automat
public partial class FindWindow : AnimatedWindow
{
    private readonly Func<TextEditor?> _getEditor;

    public FindWindow(Func<TextEditor?> getEditor)
    {
        InitializeComponent();
        _getEditor = getEditor;
        // Slide-down suplimentar la deschidere
        ContentRendered += (_, _) =>
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            ContentTranslate.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(-15, 0, new Duration(TimeSpan.FromSeconds(0.25)))
                { EasingFunction = ease });
            TxtFind.Focus();
        };
    }

    private void TxtFind_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) FindNext_Click(sender, e);
    }

    private StringComparison Comparison =>
        ChkMatchCase.IsChecked == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    private void FindNext_Click(object sender, RoutedEventArgs e)
    {
        var editor = _getEditor();
        if (editor == null || string.IsNullOrEmpty(TxtFind.Text)) return;

        var text  = editor.Text;
        var term  = TxtFind.Text;
        var start = editor.SelectionStart + editor.SelectionLength;
        var idx   = text.IndexOf(term, start, Comparison);
        if (idx < 0) idx = text.IndexOf(term, 0, Comparison);

        if (idx >= 0)
        {
            editor.Select(idx, term.Length);
            editor.ScrollToLine(editor.Document.GetLineByOffset(idx).LineNumber);
        }
        else
            MessageBox.Show($"'{term}' not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void FindPrev_Click(object sender, RoutedEventArgs e)
    {
        var editor = _getEditor();
        if (editor == null || string.IsNullOrEmpty(TxtFind.Text)) return;

        var text  = editor.Text;
        var term  = TxtFind.Text;
        var start = editor.SelectionStart - 1;
        if (start < 0) start = text.Length - 1;
        var idx = text.LastIndexOf(term, start, Comparison);
        if (idx < 0) idx = text.LastIndexOf(term, Comparison);

        if (idx >= 0)
        {
            editor.Select(idx, term.Length);
            editor.ScrollToLine(editor.Document.GetLineByOffset(idx).LineNumber);
        }
        else
            MessageBox.Show($"'{term}' not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
