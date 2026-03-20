using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ICSharpCode.AvalonEdit;
using MyNotepad.Core;
using MyNotepad.Features.File;

namespace MyNotepad.Features.Search;


public partial class ReplaceWindow : AnimatedWindow
{
    private readonly AppViewModel _app;
    private readonly MainWindow _main;

    public ReplaceWindow(AppViewModel app, MainWindow main)
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

    private StringComparison Comparison =>
        ChkMatchCase.IsChecked == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    private void Replace_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(TxtFind.Text)) return;

        var term = TxtFind.Text;
        var replace = TxtReplace.Text;

        if (ChkAllTabs.IsChecked == true)
        {

            var tabs = _app.OpenTabs.ToList();
            if (tabs.Count == 0) return;

            int startIndex = _app.ActiveTab != null ? tabs.IndexOf(_app.ActiveTab) : 0;
            if (startIndex < 0) startIndex = 0;

            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[(startIndex + i) % tabs.Count];
                int idx = tab.Text.IndexOf(term, Comparison);
                if (idx >= 0)
                {

                    _app.ActiveTab = tab;
                    Dispatcher.InvokeAsync(() =>
                    {
                        var ed = _main.GetActiveEditor();
                        if (ed != null)
                        {
                            ed.Document.Replace(idx, term.Length, replace);
                            ed.Select(idx, replace.Length);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Loaded);
                    return;
                }
            }

            MessageBox.Show($"'{term}' not found in any tab.", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var editor = _main.GetActiveEditor();
            if (editor == null) return;

            var idx = editor.Text.IndexOf(term, Comparison);
            if (idx >= 0)
            {
                editor.Document.Replace(idx, term.Length, replace);
                editor.Select(idx, replace.Length);
            }
            else
                MessageBox.Show($"'{term}' not found.", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ReplaceAll_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(TxtFind.Text)) return;

        var term = TxtFind.Text;
        var replace = TxtReplace.Text;
        int totalCount = 0;

        if (ChkAllTabs.IsChecked == true)
        {

            foreach (var tab in _app.OpenTabs.ToList())
            {
                int count = 0;
                var text = tab.Text;
                int idx = 0;
                while ((idx = text.IndexOf(term, idx, Comparison)) >= 0)
                {
                    text = text.Remove(idx, term.Length).Insert(idx, replace);
                    idx += replace.Length;
                    count++;
                }
                if (count > 0)
                {
                    tab.Text = text;
                    totalCount += count;
                }
            }

            MessageBox.Show($"Replaced {totalCount} occurrence(s) in all tabs.", "Replace All",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var editor = _main.GetActiveEditor();
            if (editor == null) return;

            int idx = 0;
            while ((idx = editor.Text.IndexOf(term, idx, Comparison)) >= 0)
            {
                editor.Document.Replace(idx, term.Length, replace);
                idx += replace.Length;
                totalCount++;
            }

            MessageBox.Show($"Replaced {totalCount} occurrence(s).", "Replace All",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}