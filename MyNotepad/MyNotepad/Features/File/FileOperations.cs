using System.IO;
using System.Windows;
using Microsoft.Win32;
using MyNotepad.Core;

namespace MyNotepad.Features.File;

public class FileOperations
{
    private readonly AppViewModel _app;

    public FileOperations(AppViewModel app)
    {
        _app = app;
    }


    public void NewDocument()
    {

        int number = 1;
        while (_app.OpenTabs.Any(t => t.FileName == "File " + number))
            number++;

        var doc = new DocumentTab();
        doc.FileName = "File " + number;
        _app.OpenTabs.Add(doc);
        _app.ActiveTab = doc;
    }


    public void OpenDocument()
    {

        var dialog = new OpenFileDialog();
        dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

        if (dialog.ShowDialog() != true) return;

        try
        {
            var doc = new DocumentTab();

            doc.Text = System.IO.File.ReadAllText(dialog.FileName);
            doc.FilePath = dialog.FileName;
            doc.FileName = Path.GetFileName(dialog.FileName);
            doc.IsDirty = false;

            _app.OpenTabs.Add(doc);
            _app.ActiveTab = doc;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error opening file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public void OpenSpecificDocument(string filePath)
    {

        var existingTab = _app.OpenTabs.FirstOrDefault(t => t.FilePath == filePath);
        if (existingTab != null)
        {
            _app.ActiveTab = existingTab;
            return;
        }

        try
        {
            var doc = new DocumentTab();

            doc.Text = System.IO.File.ReadAllText(filePath);
            doc.FilePath = filePath;
            doc.FileName = Path.GetFileName(filePath);
            doc.IsDirty = false;

            _app.OpenTabs.Add(doc);
            _app.ActiveTab = doc;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error opening file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public void SaveDocument(DocumentTab doc)
    {

        if (string.IsNullOrEmpty(doc.FilePath))
        {

            var dialog = new SaveFileDialog();
            dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dialog.DefaultExt = "txt";
            dialog.FileName = doc.FileName;

            if (dialog.ShowDialog() != true) return;

            doc.FilePath = dialog.FileName;
            doc.FileName = Path.GetFileName(dialog.FileName);
        }

        try
        {

            System.IO.File.WriteAllText(doc.FilePath, doc.Text);
            doc.IsDirty = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public void SaveDocumentAs(DocumentTab doc)
    {

        var dialog = new SaveFileDialog();
        dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
        dialog.DefaultExt = "txt";
        dialog.FileName = doc.FileName;

        if (dialog.ShowDialog() != true) return;

        doc.FilePath = dialog.FileName;
        doc.FileName = Path.GetFileName(dialog.FileName);

        try
        {

            System.IO.File.WriteAllText(doc.FilePath, doc.Text);
            doc.IsDirty = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public bool CloseDocument(DocumentTab doc)
    {
        if (doc.IsDirty)
        {
            var result = MessageBox.Show(
                "Do you want to save changes to " + doc.FileName + "?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel) return false;
            if (result == MessageBoxResult.Yes)
            {
                SaveDocument(doc);
                if (doc.IsDirty) return false;
            }
        }

        _app.OpenTabs.Remove(doc);
        return true;
    }


    public void CloseAllDocuments()
    {
        var allTabs = _app.OpenTabs.ToList();
        foreach (var doc in allTabs)
        {
            if (!CloseDocument(doc)) break;
        }
    }
}
