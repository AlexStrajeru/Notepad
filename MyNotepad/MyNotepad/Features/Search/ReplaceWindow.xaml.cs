using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ICSharpCode.AvalonEdit;
using MyNotepad.Core;

namespace MyNotepad.Features.Search;

// Mosteneste AnimatedWindow — fade-in si dark title bar automat
public partial class ReplaceWindow : AnimatedWindow
{
    private readonly Func<TextEditor?> _getEditor;

    public ReplaceWindow(Func<TextEditor?> getEditor)
    {
        InitializeComponent();
        _getEditor = getEditor;
        ContentRendered += (_, _) =>
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            ContentTranslate.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(-15, 0, new Duration(TimeSpan.FromSeconds(0.25)))
                { EasingFunction = ease });
            TxtFind.Focus();
        };
    }

    private StringComparison Comparison =>
        ChkMatchCase.IsChecked == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    private void Replace_Click(object sender, RoutedEventArgs e)
    {
        var editor = _getEditor();
        if (editor == null || string.IsNullOrEmpty(TxtFind.Text)) return;

        var term    = TxtFind.Text;
        var replace = TxtReplace.Text;
        var idx     = editor.Text.IndexOf(term, Comparison);

        if (idx >= 0)
        {
            editor.Document.Replace(idx, term.Length, replace);
            editor.Select(idx, replace.Length);
        }
        else
            MessageBox.Show($"'{term}' not found.", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ReplaceAll_Click(object sender, RoutedEventArgs e)
    {
        var editor = _getEditor();
        if (editor == null || string.IsNullOrEmpty(TxtFind.Text)) return;

        var term    = TxtFind.Text;
        var replace = TxtReplace.Text;
        int count   = 0;
        int idx     = 0;

        while ((idx = editor.Text.IndexOf(term, idx, Comparison)) >= 0)
        {
            editor.Document.Replace(idx, term.Length, replace);
            idx += replace.Length;
            count++;
        }

        MessageBox.Show($"Replaced {count} occurrence(s).", "Replace All",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}