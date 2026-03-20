using MyNotepad.Core;

namespace MyNotepad.Features.StatusBar;


public class StatusBarViewModel : ObservableObject
{
    private readonly AppViewModel _app;

    public StatusBarViewModel(AppViewModel app)
    {
        _app = app;

        _app.PropertyChanged += OnAppPropertyChanged;
    }


    private void OnAppPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppViewModel.ActiveTab))
        {
            if (_app.ActiveTab != null)
                _app.ActiveTab.PropertyChanged += OnActiveTabPropertyChanged;
            OnPropertyChanged(string.Empty);
        }
    }


    private void OnActiveTabPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(string.Empty);
    }


    public string FileName
    {
        get
        {
            if (_app.ActiveTab == null) return "No file open";
            return _app.ActiveTab.FileName;
        }
    }


    public string FilePath
    {
        get
        {
            if (_app.ActiveTab == null) return "";
            if (_app.ActiveTab.FilePath == "") return "Not saved";
            return _app.ActiveTab.FilePath;
        }
    }


    public string LineCount
    {
        get
        {
            if (_app.ActiveTab == null) return "Lines: 0";
            int lines = _app.ActiveTab.Text.Split('\n').Length;
            return "Lines: " + lines;
        }
    }


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
