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

    // Utilizeaza functiile predefinite din componenta TextEditor a librariei AvalonEdit.
    private void UndoButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Undo();
    private void RedoButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Redo();
    private void CutButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Cut();
    private void CopyButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Copy();
    private void PasteButton_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Paste();

    // Evenimente Folder Explorer

    private void FolderTreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (FolderTreeView.SelectedItem is MyNotepad.Features.Explorer.ExplorerItemViewModel item)
        {
            if (!item.IsDirectory && DataContext is AppViewModel app)
            {
                app.FileOperations.OpenSpecificDocument(item.FullPath);
            }
        }
    }

    private void StackPanel_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if ((sender as StackPanel)?.DataContext is Features.Explorer.ExplorerItemViewModel item && !item.IsDirectory)
        {
            e.Handled = true; // Opreste afisarea meniului contextual pe elementele care nu sunt directoare.
        }
    }

    private void FolderContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
        {
            foreach (var item in menu.Items)
            {
                if (item is MenuItem menuItem && menuItem.Header?.ToString() == "Paste folder")
                {
                    menuItem.IsEnabled = !string.IsNullOrEmpty(_copiedFolderPath) && System.IO.Directory.Exists(_copiedFolderPath);
                }
            }
        }
    }

    private void FolderContextMenu_NewFile(object sender, RoutedEventArgs e)
    {
        if ((sender as MenuItem)?.DataContext is Features.Explorer.ExplorerItemViewModel item && item.IsDirectory)
        {
            try
            {
                // Manipuleaza cai directoare si creeaza fisiere fizic prin libraria nativa System.IO.
                string newFilePath = System.IO.Path.Combine(item.FullPath, "NewFile.txt");
                int counter = 1;
                while (System.IO.File.Exists(newFilePath))
                {
                    newFilePath = System.IO.Path.Combine(item.FullPath, $"NewFile ({counter}).txt");
                    counter++;
                }
                System.IO.File.WriteAllText(newFilePath, "");
                item.LoadChildren(); // Reincarca elementele pentru a afisa fisierul nou in interfata.
                
                if (DataContext is AppViewModel app)
                    app.FileOperations.OpenSpecificDocument(newFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void FolderContextMenu_CopyPath(object sender, RoutedEventArgs e)
    {
        if ((sender as MenuItem)?.DataContext is Features.Explorer.ExplorerItemViewModel item && item.IsDirectory)
        {
            // Memoreaza in registrul sistemului calea folosind clasa Clipboard din libraria System.Windows.
            Clipboard.SetText(item.FullPath);
        }
    }

    private static string? _copiedFolderPath = null;

    private void FolderContextMenu_CopyFolder(object sender, RoutedEventArgs e)
    {
        if ((sender as MenuItem)?.DataContext is Features.Explorer.ExplorerItemViewModel item && item.IsDirectory)
        {
            _copiedFolderPath = item.FullPath;
        }
    }

    private void FolderContextMenu_PasteFolder(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_copiedFolderPath) || !System.IO.Directory.Exists(_copiedFolderPath)) return;

        if ((sender as MenuItem)?.DataContext is Features.Explorer.ExplorerItemViewModel item && item.IsDirectory)
        {
            try
            {
                string folderName = System.IO.Path.GetFileName(_copiedFolderPath);
                string targetPath = System.IO.Path.Combine(item.FullPath, folderName);
                
                // Opreste copierea directorului in el insusi pentru a preveni o bucla infinita de fisiere.
                if (targetPath.StartsWith(_copiedFolderPath, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Cannot copy a folder into itself.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int counter = 1;
                string originalTargetPath = targetPath;
                while (System.IO.Directory.Exists(targetPath))
                {
                    targetPath = $"{originalTargetPath} ({counter})";
                    counter++;
                }

                CopyDirectory(_copiedFolderPath, targetPath);
                item.LoadChildren(); // Reincarca elementele pentru a afisa folderul nou in interfata.
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not paste folder: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        // Copiaza recursiv directoarele folosind extensiile native File si Directory din System.IO.
        System.IO.Directory.CreateDirectory(destDir);
        foreach (var file in System.IO.Directory.GetFiles(sourceDir))
            System.IO.File.Copy(file, System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(file)));
        foreach (var directory in System.IO.Directory.GetDirectories(sourceDir))
            CopyDirectory(directory, System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(directory)));
    }
}