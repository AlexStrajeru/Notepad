namespace MyNotepad.ViewModels;

public class DocumentViewModel : ViewModelBase
{
    private string _text = string.Empty;
    private string _fileName = "New File";
    private string? _filePath;
    private bool _isDirty;
    
    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value))
            {
                IsDirty = true;
            }
        }
    }
    
    public string FileName
    {
        get => _fileName;
        set
        {
            if (SetProperty(ref _fileName, value))
            {
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }
    
    public string? FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }
    
    public bool IsDirty
    {
        get => _isDirty;
        set
        { 
            if(SetProperty(ref _isDirty, value))
            {
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }
    
    public string DisplayTitle => $"{FileName}{(IsDirty ? "*" : "")}";
}