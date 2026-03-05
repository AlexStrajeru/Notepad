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

    // Creeaza un tab nou gol cu numele "File 1", "File 2" etc.
    public void NewDocument()
    {
        // Gaseste primul numar liber (daca ai File 1 si File 3, va face File 2)
        int number = 1;
        while (_app.OpenTabs.Any(t => t.FileName == "File " + number))
            number++;

        var doc = new DocumentTab();
        doc.FileName = "File " + number;
        _app.OpenTabs.Add(doc);
        _app.ActiveTab = doc;
    }

    // Deschide un fisier de pe disc intr-un tab nou
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

    // Salveaza fisierul curent (daca nu are cale, arata dialogul de Save As)
    public void SaveDocument(DocumentTab doc)
    {
        // Daca fisierul nu a fost salvat niciodata, cere o locatie
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
            doc.IsDirty = false; // Marcheaza fisierul ca salvat (dispare *)
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Save As — intotdeauna cere o locatie noua, chiar daca fisierul e deja salvat
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

    // Inchide un tab — daca are modificari nesalvate, intreaba utilizatorul
    public bool CloseDocument(DocumentTab doc)
    {
        if (doc.IsDirty)
        {
            var result = MessageBox.Show(
                "Do you want to save changes to " + doc.FileName + "?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel) return false; // Utilizatorul a anulat
            if (result == MessageBoxResult.Yes)
            {
                SaveDocument(doc);
                if (doc.IsDirty) return false; // Salvarea a esuat
            }
        }

        _app.OpenTabs.Remove(doc);
        return true;
    }

    // Inchide toate tab-urile deschise
    public void CloseAllDocuments()
    {
        var allTabs = _app.OpenTabs.ToList();
        foreach (var doc in allTabs)
        {
            if (!CloseDocument(doc)) break; // Opreste daca utilizatorul a apasas Cancel
        }
    }
}
