using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MyNotepad.Core;

namespace MyNotepad.Features.Explorer;

public class ExplorerItemViewModel : ObservableObject
{
    private bool _isExpanded;
    private bool _isLoaded;

    public string Name { get; }
    public string FullPath { get; }
    public bool IsDirectory { get; }
    
    // Stabileste iconita afisata in functie de tipul elementului.
    public string Icon => IsDirectory ? "📁" : "📄";

    public ObservableCollection<ExplorerItemViewModel> Children { get; } = new ObservableCollection<ExplorerItemViewModel>();

    // Comenzi pentru context menu
    public ICommand NewFileCommand { get; }
    public ICommand CopyPathCommand { get; }
    public ICommand CopyFolderCommand { get; }
    public ICommand PasteFolderCommand { get; }
    public ICommand OpenDocumentCommand { get; }

    // Memoreaza static folderul copiat
    private static string? _copiedFolderPath = null;

    // Creeaza un element temporar folosit inainte de incarcarea datelor reale.
    private ExplorerItemViewModel(string dummyName)
    {
        Name = dummyName;
        FullPath = "";
        IsDirectory = false;
    }

    public ExplorerItemViewModel(string fullPath, bool isDirectory)
    {
        FullPath = fullPath;
        IsDirectory = isDirectory;
        Name = Path.GetFileName(fullPath);
        if (string.IsNullOrEmpty(Name))
            Name = fullPath; // Foloseste calea completa daca elementul este o partitie din sistem sau o cale principala.

        if (IsDirectory)
        {
            // Adauga un element temporar pentru a forta afisarea sagetii de expandare in interfata.
            Children.Add(new ExplorerItemViewModel("Loading..."));
        }

        NewFileCommand = new RelayCommand(CreateNewFile, () => IsDirectory);
        CopyPathCommand = new RelayCommand(CopyPath, () => IsDirectory);
        CopyFolderCommand = new RelayCommand(CopyFolder, () => IsDirectory);
        PasteFolderCommand = new RelayCommand(PasteFolder, () => IsDirectory && !string.IsNullOrEmpty(_copiedFolderPath) && Directory.Exists(_copiedFolderPath));
        OpenDocumentCommand = new RelayCommand(OpenDocument, () => !IsDirectory);
    }

    // Incarca asincron continutul directorului doar in momentul in care utilizatorul il extinde manual.
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value))
            {
                if (_isExpanded && !_isLoaded && IsDirectory)
                {
                    LoadChildren();
                }
            }
        }
    }

    public void LoadChildren()
    {
        Children.Clear();
        if (!IsDirectory) return;

        try
        {
            // Extrage fisierele si folderele pe baza caii utilizand clasa Directory din libraria System.IO.
            var dirs = Directory.GetDirectories(FullPath).OrderBy(d => d).ToList();
            var files = Directory.GetFiles(FullPath).OrderBy(f => f).ToList();

            foreach (var dir in dirs)
                Children.Add(new ExplorerItemViewModel(dir, true));

            foreach (var file in files)
                Children.Add(new ExplorerItemViewModel(file, false));

            _isLoaded = true;
        }
        catch (Exception)
        {
            // Ignora directoarele care necesita drepturi de administrator.
        }
    }

    private void CreateNewFile()
    {
        try
        {
            string newFilePath = Path.Combine(FullPath, "NewFile.txt");
            int counter = 1;
            while (File.Exists(newFilePath))
            {
                newFilePath = Path.Combine(FullPath, $"NewFile ({counter}).txt");
                counter++;
            }
            File.WriteAllText(newFilePath, "");
            LoadChildren();
            
            // Notifica aplicatia principala sa deschida fisierul creat (trimitem mesaj global)
            Application.Current.Dispatcher.Invoke(() => 
            {
                if (Application.Current.MainWindow?.DataContext is AppViewModel app)
                    app.FileOperations.OpenSpecificDocument(newFilePath);
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not create file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CopyPath()
    {
        Clipboard.SetText(FullPath);
    }

    private void CopyFolder()
    {
        _copiedFolderPath = FullPath;
    }

    private void PasteFolder()
    {
        if (string.IsNullOrEmpty(_copiedFolderPath) || !Directory.Exists(_copiedFolderPath)) return;

        try
        {
            string folderName = Path.GetFileName(_copiedFolderPath);
            string targetPath = Path.Combine(FullPath, folderName);
            
            if (targetPath.StartsWith(_copiedFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Cannot copy a folder into itself.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int counter = 1;
            string originalTargetPath = targetPath;
            while (Directory.Exists(targetPath))
            {
                targetPath = $"{originalTargetPath} ({counter})";
                counter++;
            }

            CopyDirectory(_copiedFolderPath, targetPath);
            LoadChildren();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not paste folder: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)));
        foreach (var directory in Directory.GetDirectories(sourceDir))
            CopyDirectory(directory, Path.Combine(destDir, Path.GetFileName(directory)));
    }

    private void OpenDocument()
    {
        if (IsDirectory) return;

        Application.Current.Dispatcher.Invoke(() => 
        {
            if (Application.Current.MainWindow?.DataContext is AppViewModel app)
                app.FileOperations.OpenSpecificDocument(FullPath);
        });
    }
}
