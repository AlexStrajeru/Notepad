using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using MyNotepad.Core;
using MyNotepad.Features.File;

namespace MyNotepad;

// Mosteneste AnimatedWindow — fade-in si dark title bar automat
public partial class MainWindow : AnimatedWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Editor_Loaded(object sender, RoutedEventArgs e)
    {
        var editor = (TextEditor)sender;

        // Centrare numere linii
        var lineNumberMargin = editor.TextArea.LeftMargins
            .OfType<LineNumberMargin>()
            .FirstOrDefault();

        if (lineNumberMargin != null)
        {
            lineNumberMargin.Margin = new Thickness(6, 0, 6, 0);
        }

        // Leaga editorul la DocumentTab-ul corect
        // Se apeleaza si cand se schimba tab-ul activ (DataContext se schimba)
        BindEditorToDoc(editor);
        editor.DataContextChanged += (_, _) => BindEditorToDoc(editor);
    }

    private void BindEditorToDoc(TextEditor editor)
    {
        if (editor.DataContext is not DocumentTab doc) return;

        editor.LineNumbersForeground = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        // Incarca textul din DocumentTab in editor
        if (editor.Text != doc.Text)
            editor.Text = doc.Text;

        // Dezaboneaza orice handler vechi pentru a evita leak-uri
        editor.TextChanged -= OnEditorTextChanged;
        editor.TextChanged += OnEditorTextChanged;
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (sender is TextEditor editor && editor.DataContext is DocumentTab doc)
            doc.Text = editor.Text;
    }

    public TextEditor? GetActiveEditor() => FindVisualChild<TextEditor>(MainTabControl);

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result) return result;
            var found = FindVisualChild<T>(child);
            if (found != null) return found;
        }
        return null;
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Undo();
    private void RedoButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Redo();
    private void CutButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Cut();
    private void CopyButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Copy();
    private void PasteButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Paste();
}