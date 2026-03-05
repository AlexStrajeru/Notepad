using MyNotepad.Core;

namespace MyNotepad.Features.File;

// Reprezinta un singur tab deschis in editor
public class DocumentTab : ObservableObject
{
    private string _text = "";
    private string _fileName = "New File";
    private string _filePath = "";
    private bool _isDirty = false;

    // Textul din editor
    public string Text
    {
        get { return _text; }
        set
        {
            if (SetProperty(ref _text, value))
                IsDirty = true; // orice modificare marcheaza fisierul ca nesalvat
        }
    }

    // Numele fisierului (ex: "File 1" sau "document.txt")
    public string FileName
    {
        get { return _fileName; }
        set
        {
            if (SetProperty(ref _fileName, value))
                OnPropertyChanged(nameof(DisplayTitle)); // actualizeaza si titlul tab-ului
        }
    }

    // Calea completa pe disc (ex: "C:\Users\...\document.txt")
    // Ramane gol daca fisierul nu a fost salvat niciodata
    public string FilePath
    {
        get { return _filePath; }
        set { SetProperty(ref _filePath, value); }
    }

    // true = fisierul are modificari nesalvate
    public bool IsDirty
    {
        get { return _isDirty; }
        set
        {
            if (SetProperty(ref _isDirty, value))
                OnPropertyChanged(nameof(DisplayTitle)); // actualizeaza titlul (adauga/scoate *)
        }
    }

    // Titlul afisat pe tab — adauga * daca fisierul are modificari nesalvate
    public string DisplayTitle
    {
        get
        {
            if (IsDirty) return FileName + "*";
            return FileName;
        }
    }
}
