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

    // Creeaza un tab gol si genereaza un nume automat.
    public void NewDocument()
    {
        // Gaseste primul numar disponibil pentru noul fisier creat.
        int number = 1;
        while (_app.OpenTabs.Any(t => t.FileName == "File " + number))
            number++;

        var doc = new DocumentTab();
        doc.FileName = "File " + number;
        _app.OpenTabs.Add(doc);
        _app.ActiveTab = doc;
    }

    // Deschide un fisier selectat de pe disc intr-un tab nou.
    public void OpenDocument()
    {
        // Utilizeaza componenta OpenFileDialog din libraria Microsoft.Win32.
        var dialog = new OpenFileDialog();
        dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

        if (dialog.ShowDialog() != true) return;

        try
        {
            var doc = new DocumentTab();
            // Citeste textul integral utilizand clasa File din libraria System.IO.
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

    // Deschide intr-un tab nou fisierul specificat la adresa data.
    public void OpenSpecificDocument(string filePath)
    {
        // Verifica daca fisierul este deja deschis intr-un tab existent.
        var existingTab = _app.OpenTabs.FirstOrDefault(t => t.FilePath == filePath);
        if (existingTab != null)
        {
            _app.ActiveTab = existingTab;
            return;
        }

        try
        {
            var doc = new DocumentTab();
            // Incarca continutul fisierului utilizand clasa File din libraria System.IO.
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

    // Salveaza fisierul curent sau deschide dialogul de salvare daca este un fisier nou.
    public void SaveDocument(DocumentTab doc)
    {
        // Verifica daca fisierul a mai fost salvat anterior.
        if (string.IsNullOrEmpty(doc.FilePath))
        {
            // Utilizeaza componenta SaveFileDialog din libraria Microsoft.Win32.
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
            // Rescrie intregul continut in fisier utilizand clasa File din libraria System.IO.
            System.IO.File.WriteAllText(doc.FilePath, doc.Text);
            doc.IsDirty = false; // Elimina marcajul de continut nesalvat.
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Deschide dialogul de salvare pentru a alege o locatie noua.
    public void SaveDocumentAs(DocumentTab doc)
    {
        // Apeleaza componenta nativa SaveFileDialog din libraria Microsoft.Win32.
        var dialog = new SaveFileDialog();
        dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
        dialog.DefaultExt = "txt";
        dialog.FileName = doc.FileName;

        if (dialog.ShowDialog() != true) return;

        doc.FilePath = dialog.FileName;
        doc.FileName = Path.GetFileName(dialog.FileName);

        try
        {
            // Salveaza stringul in noul fisier utilizand clasa File din libraria System.IO.
            System.IO.File.WriteAllText(doc.FilePath, doc.Text);
            doc.IsDirty = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Inchide un tab si solicita confirmare daca exista modificari nesalvate.
    public bool CloseDocument(DocumentTab doc)
    {
        if (doc.IsDirty)
        {
            var result = MessageBox.Show(
                "Do you want to save changes to " + doc.FileName + "?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel) return false; // Renunta la operatia de inchidere.
            if (result == MessageBoxResult.Yes)
            {
                SaveDocument(doc);
                if (doc.IsDirty) return false; // Opreste inchiderea daca salvarea fisierului a eșuat.
            }
        }

        _app.OpenTabs.Remove(doc);
        return true;
    }

    // Inchide toate documentele deschise in fereastra principala.
    public void CloseAllDocuments()
    {
        var allTabs = _app.OpenTabs.ToList();
        foreach (var doc in allTabs)
        {
            if (!CloseDocument(doc)) break; // Intrerupe procesul daca actiunea este anulata de utilizator.
        }
    }
}
