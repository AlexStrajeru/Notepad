using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ICSharpCode.AvalonEdit;
using MyNotepad.Core;
using MyNotepad.Features.File;

namespace MyNotepad.Features.Search;


public partial class FindWindow : AnimatedWindow
{
    private readonly AppViewModel _app;
    private readonly MainWindow _main;

    public FindWindow(AppViewModel app, MainWindow main)
    {
        InitializeComponent();
        _app = app;
        _main = main;

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
        if (string.IsNullOrEmpty(TxtFind.Text)) return;
        var term = TxtFind.Text;

        if (ChkAllTabs.IsChecked == true)
        {

            var tabs = _app.OpenTabs.ToList();
            if (tabs.Count == 0) return;

            int startIndex = _app.ActiveTab != null ? tabs.IndexOf(_app.ActiveTab) : 0;
            if (startIndex < 0) startIndex = 0;


            var editor = _main.GetActiveEditor();
            if (editor != null && _app.ActiveTab != null)
            {
                int searchFrom = editor.SelectionStart + editor.SelectionLength;
                int idx = _app.ActiveTab.Text.IndexOf(term, searchFrom, Comparison);
                if (idx >= 0)
                {
                    editor.Select(idx, term.Length);
                    editor.ScrollToLine(editor.Document.GetLineByOffset(idx).LineNumber);
                    return;
                }
            }


            for (int i = 1; i < tabs.Count; i++)
            {
                var tab = tabs[(startIndex + i) % tabs.Count];
                int idx = tab.Text.IndexOf(term, 0, Comparison);
                if (idx >= 0)
                {
                    _app.ActiveTab = tab;

                    Dispatcher.InvokeAsync(() =>
                    {
                        var ed = _main.GetActiveEditor();
                        if (ed != null)
                        {
                            ed.Select(idx, term.Length);
                            ed.ScrollToLine(ed.Document.GetLineByOffset(idx).LineNumber);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Loaded);
                    return;
                }
            }


            MessageBox.Show($"'{term}' not found in any tab.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {

            var editor = _main.GetActiveEditor();
            if (editor == null) return;

            var text = editor.Text;
            var start = editor.SelectionStart + editor.SelectionLength;
            var idx = text.IndexOf(term, start, Comparison);
            if (idx < 0) idx = text.IndexOf(term, 0, Comparison);

            if (idx >= 0)
            {
                editor.Select(idx, term.Length);
                editor.ScrollToLine(editor.Document.GetLineByOffset(idx).LineNumber);
            }
            else
            {

                MessageBox.Show($"'{term}' not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void FindPrev_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(TxtFind.Text)) return;
        var term = TxtFind.Text;

        if (ChkAllTabs.IsChecked == true)
        {
            var tabs = _app.OpenTabs.ToList();
            if (tabs.Count == 0) return;

            int startIndex = _app.ActiveTab != null ? tabs.IndexOf(_app.ActiveTab) : 0;
            if (startIndex < 0) startIndex = 0;


            var editor = _main.GetActiveEditor();
            if (editor != null && _app.ActiveTab != null)
            {
                int searchTo = editor.SelectionStart - 1;
                if (searchTo >= 0)
                {
                    int idx = _app.ActiveTab.Text.LastIndexOf(term, searchTo, Comparison);
                    if (idx >= 0)
                    {
                        editor.Select(idx, term.Length);
                        editor.ScrollToLine(editor.Document.GetLineByOffset(idx).LineNumber);
                        return;
                    }
                }
            }


            for (int i = 1; i < tabs.Count; i++)
            {
                var tab = tabs[(startIndex - i + tabs.Count) % tabs.Count];
                if (tab.Text.Length == 0) continue;
                int idx = tab.Text.LastIndexOf(term, tab.Text.Length - 1, Comparison);
                if (idx >= 0)
                {
                    _app.ActiveTab = tab;
                    Dispatcher.InvokeAsync(() =>
                    {
                        var ed = _main.GetActiveEditor();
                        if (ed != null)
                        {
                            ed.Select(idx, term.Length);
                            ed.ScrollToLine(ed.Document.GetLineByOffset(idx).LineNumber);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Loaded);
                    return;
                }
            }

            MessageBox.Show($"'{term}' not found in any tab.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var editor = _main.GetActiveEditor();
            if (editor == null) return;

            var text = editor.Text;
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
}
