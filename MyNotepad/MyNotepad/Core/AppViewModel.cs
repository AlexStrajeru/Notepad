using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyNotepad.Features.File;
using MyNotepad.Features.StatusBar;
using MyNotepad.Features.Help;

namespace MyNotepad.Core;

public class AppViewModel : ObservableObject
{
    public ObservableCollection<DocumentTab> OpenTabs { get; } = new ObservableCollection<DocumentTab>();

    private DocumentTab? _activeTab;
    public DocumentTab? ActiveTab
    {
        get { return _activeTab; }
        set { SetProperty(ref _activeTab, value); }
    }

    public int DocumentCounter { get; set; } = 1;

    public FileOperations FileOperations { get; }
    public StatusBarViewModel StatusBar { get; }

    public ICommand NewCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand CloseAllCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand AboutCommand { get; }

    public AppViewModel()
    {
        FileOperations = new FileOperations(this);
        StatusBar = new StatusBarViewModel(this);

        NewCommand      = new RelayCommand(NewDocument);
        OpenCommand     = new RelayCommand(OpenDocument);
        SaveCommand     = new RelayCommand(SaveDocument, CanSave);
        CloseCommand    = new RelayCommand<DocumentTab>(CloseDocument);
        CloseAllCommand = new RelayCommand(CloseAllDocuments, CanCloseAll);
        ExitCommand     = new RelayCommand(ExitApplication);
        AboutCommand    = new RelayCommand(OpenAbout);

        FileOperations.NewDocument();
    }

    private void NewDocument()
    {
        FileOperations.NewDocument();
    }

    private void OpenDocument()
    {
        FileOperations.OpenDocument();
    }

    private void SaveDocument()
    {
        if (ActiveTab != null)
            FileOperations.SaveDocument(ActiveTab);
    }

    private bool CanSave()
    {
        return ActiveTab != null;
    }

    private void CloseDocument(DocumentTab tab)
    {
        FileOperations.CloseDocument(tab);
    }

    private void CloseAllDocuments()
    {
        FileOperations.CloseAllDocuments();
    }

    private bool CanCloseAll()
    {
        return OpenTabs.Count > 0;
    }

    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }

    private void OpenAbout()
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.Owner = Application.Current.MainWindow;
        aboutWindow.ShowDialog();
    }
}
