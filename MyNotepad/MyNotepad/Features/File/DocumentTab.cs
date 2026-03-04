using MyNotepad.Core;

namespace MyNotepad.Features.File;

public class DocumentTab : ObservableObject
{
    private string _text = "";
    private string _fileName = "New File";
    private string _filePath = "";
    private bool _isDirty = false;

    public string Text
    {
        get { return _text; }
        set
        {
            if (SetProperty(ref _text, value))
                IsDirty = true;
        }
    }

    public string FileName
    {
        get { return _fileName; }
        set
        {
            if (SetProperty(ref _fileName, value))
                OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    public string FilePath
    {
        get { return _filePath; }
        set { SetProperty(ref _filePath, value); }
    }

    public bool IsDirty
    {
        get { return _isDirty; }
        set
        {
            if (SetProperty(ref _isDirty, value))
                OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    public string DisplayTitle
    {
        get
        {
            if (IsDirty)
                return FileName + "*";
            return FileName;
        }
    }
}
