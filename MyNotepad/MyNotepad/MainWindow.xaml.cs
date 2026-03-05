using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using MyNotepad.Features.File;

namespace MyNotepad;

public partial class MainWindow : Window
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        int dark = 1;
        DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(int)); // Windows 11
        DwmSetWindowAttribute(hwnd, 19, ref dark, sizeof(int)); // Windows 10
    }

    private void Editor_Loaded(object sender, RoutedEventArgs e)
    {
        var editor = (TextEditor)sender;
        var doc = (DocumentTab)editor.DataContext;

        editor.LineNumbersForeground = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        editor.Text = doc.Text;

        editor.TextChanged += (s, args) =>
        {
            doc.Text = editor.Text;
        };

        doc.PropertyChanged += (s, args) =>
        {
            if (args.PropertyName == nameof(DocumentTab.Text) && editor.Text != doc.Text)
                editor.Text = doc.Text;
        };
    }

    private TextEditor? GetActiveEditor()
    {
        return FindVisualChild<TextEditor>(MainTabControl);
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;
            var found = FindVisualChild<T>(child);
            if (found != null)
                return found;
        }
        return null;
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Undo();
    private void RedoButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Redo();
    private void CutButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Cut();
    private void CopyButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Copy();
    private void PasteButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Paste();
}