using MyNotepad.Core;

namespace MyNotepad.Features.StatusBar;

// Datele afisate in bara de jos a ferestrei (nume fisier, cale, linii, status salvare)
public class StatusBarViewModel : ObservableObject
{
    private readonly AppViewModel _app;

    public StatusBarViewModel(AppViewModel app)
    {
        _app = app;
        // Asculta schimbarea tab-ului activ
        _app.PropertyChanged += OnAppPropertyChanged;
    }

    // Cand se schimba tab-ul activ, actualizeaza toate proprietatile barei
    private void OnAppPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppViewModel.ActiveTab))
        {
            if (_app.ActiveTab != null)
                _app.ActiveTab.PropertyChanged += OnActiveTabPropertyChanged;
            OnPropertyChanged(string.Empty); // string.Empty = actualizeaza tot
        }
    }

    // Cand se schimba ceva in tab-ul activ (text, saved etc.), actualizeaza bara
    private void OnActiveTabPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(string.Empty);
    }

    // Numele fisierului afisat in bara
    public string FileName
    {
        get
        {
            if (_app.ActiveTab == null) return "No file open";
            return _app.ActiveTab.FileName;
        }
    }

    // Calea fisierului sau "Not saved" daca nu a fost salvat
    public string FilePath
    {
        get
        {
            if (_app.ActiveTab == null) return "";
            if (_app.ActiveTab.FilePath == "") return "Not saved";
            return _app.ActiveTab.FilePath;
        }
    }

    // Numarul de linii din fisierul curent
    public string LineCount
    {
        get
        {
            if (_app.ActiveTab == null) return "Lines: 0";
            int lines = _app.ActiveTab.Text.Split('\n').Length;
            return "Lines: " + lines;
        }
    }

    // "Saved" sau "Unsaved" in functie de starea fisierului
    public string DirtyStatus
    {
        get
        {
            if (_app.ActiveTab == null) return "";
            if (_app.ActiveTab.IsDirty) return "Unsaved";
            return "Saved";
        }
    }
}
