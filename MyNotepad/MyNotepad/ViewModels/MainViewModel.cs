using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MyNotepad.Commands;

namespace MyNotepad.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<DocumentViewModel> Documents { get; } = new ObservableCollection<DocumentViewModel>();

    private DocumentViewModel? _selectedDocument;
    public DocumentViewModel? SelectedDocument
    {
        get => _selectedDocument;
        set => SetProperty(ref _selectedDocument, value);
    }
    
    private int _documentCounter = 1;
    
    public ICommand NewCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand CloseAllCommand { get; }
    
    public MainViewModel()
    {
        NewCommand = new RelayCommand(_ => NewDocument());
        OpenCommand = new RelayCommand(_ => OpenDocument());
        SaveCommand = new RelayCommand(_ => SaveDocument(), _ => SelectedDocument != null);
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
        
        CloseCommand = new RelayCommand(p => CloseDocument(p as DocumentViewModel), p => p is DocumentViewModel || SelectedDocument != null);
        CloseAllCommand = new RelayCommand(_ => CloseAllDocuments(), _ => Documents.Count > 0);

        NewDocument();
    }

    private void NewDocument()
    {
        var newDoc = new DocumentViewModel()
        {
            FileName = $"File {_documentCounter}"
        };
        
        _documentCounter++;
        Documents.Add(newDoc);
        SelectedDocument = newDoc;
    }

    private void OpenDocument()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        };
        
        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var content = File.ReadAllText(openFileDialog.FileName);
                var doc = new DocumentViewModel
                {
                    Text = content,
                    FilePath = openFileDialog.FileName,
                    FileName = Path.GetFileName(openFileDialog.FileName),
                    IsDirty = false
                };
                
                Documents.Add(doc);
                SelectedDocument = doc;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SaveDocument()
    {
        var doc = SelectedDocument;
        if (doc == null) return;

        if (string.IsNullOrEmpty(doc.FilePath))
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = doc.FileName
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                doc.FilePath = saveFileDialog.FileName;
                doc.FileName = Path.GetFileName(saveFileDialog.FileName);
            }
            else return;
        }

        try
        {
            File.WriteAllText(doc.FilePath, doc.Text);
            doc.IsDirty = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private bool CloseDocument(DocumentViewModel? doc)
    {
        if (doc == null) doc = SelectedDocument;
        if (doc == null) return true;

        if (doc.IsDirty)
        {
            var result = MessageBox.Show($"Do you want to save changes to {doc.FileName}?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Cancel) return false;
            
            if (result == MessageBoxResult.Yes)
            {
                SelectedDocument = doc;
                SaveDocument();
                if (doc.IsDirty) return false; 
            }
        }
        
        Documents.Remove(doc);
        return true;
    }

    private void CloseAllDocuments()
    {
        var docsList = Documents.ToList();
        foreach (var doc in docsList)
        {
            if (!CloseDocument(doc)) break;
        }
    }
}