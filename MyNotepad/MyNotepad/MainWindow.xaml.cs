using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using MyNotepad.Core;
using MyNotepad.Features.File;

namespace MyNotepad;

public partial class MainWindow : AnimatedWindow
{
    public MainWindow()
    {
        InitializeComponent();

        AddCommandBinding(ApplicationCommands.Undo, (e) => e.Undo(), (e) => e.CanUndo);
        AddCommandBinding(ApplicationCommands.Redo, (e) => e.Redo(), (e) => e.CanRedo);
        AddCommandBinding(ApplicationCommands.Cut, (e) => e.Cut(), (e) => true);
        AddCommandBinding(ApplicationCommands.Copy, (e) => e.Copy(), (e) => true);
        AddCommandBinding(ApplicationCommands.Paste, (e) => e.Paste(), (e) => true);
    }

    private void AddCommandBinding(ICommand command, Action<TextEditor> execute, Func<TextEditor, bool> canExecute)
    {
        CommandBindings.Add(new CommandBinding(command,
            (s, e) => {
                var editor = GetActiveEditor();
                if (editor != null) execute(editor);
            },
            (s, e) => {
                var editor = GetActiveEditor();
                e.CanExecute = editor != null && canExecute(editor);
            }
        ));
    }

    private void Editor_Loaded(object sender, RoutedEventArgs e)
    {
        var editor = (TextEditor)sender;

        var lineNumberMargin = editor.TextArea.LeftMargins
            .OfType<LineNumberMargin>()
            .FirstOrDefault();

        if (lineNumberMargin != null)
        {
            lineNumberMargin.Margin = new Thickness(6, 0, 6, 0);
        }

        BindEditorToDoc(editor);
        editor.DataContextChanged += (_, _) => BindEditorToDoc(editor);
    }

    private void BindEditorToDoc(TextEditor editor)
    {
        if (editor.DataContext is not DocumentTab doc) return;

        editor.LineNumbersForeground = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        if (editor.Text != doc.Text)
            editor.Text = doc.Text;

        editor.TextChanged -= OnEditorTextChanged;
        editor.TextChanged += OnEditorTextChanged;
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (sender is TextEditor editor && editor.DataContext is DocumentTab doc)
            doc.Text = editor.Text;
    }

    public TextEditor? GetActiveEditor() => FindVisualChild<TextEditor>(MainTabControl);

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is AppViewModel vm)
        {
            vm.FileOperations.CloseAllDocuments();
            if (vm.OpenTabs.Count > 0)
            {
                e.Cancel = true;
            }
        }
        base.OnClosing(e);
    }

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

}