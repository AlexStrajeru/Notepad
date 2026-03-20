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
        set
        {
            if (_activeTab != null)
                _activeTab.PropertyChanged -= OnActiveTabPropertyChanged;

            if (SetProperty(ref _activeTab, value))
            {
                Explorer.UpdatePath(_activeTab?.FilePath);
                if (_activeTab != null)
                    _activeTab.PropertyChanged += OnActiveTabPropertyChanged;
            }
        }
    }

    private void OnActiveTabPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentTab.FilePath))
        {
            Explorer.UpdatePath(ActiveTab?.FilePath);
        }
    }

    public int DocumentCounter { get; set; } = 1;


    public FileOperations FileOperations { get; }


    public Features.Explorer.ExplorerViewModel Explorer { get; } = new Features.Explorer.ExplorerViewModel();


    public StatusBarViewModel StatusBar { get; }


    public ICommand NewCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand CloseAllCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand OpenFindCommand { get; }
    public ICommand OpenReplaceCommand { get; }
    public ICommand OpenReplaceAllCommand { get; }
    public ICommand ToggleSearchAllTabsCommand { get; }
    public ICommand ShowFolderExplorerCommand { get; }
    public ICommand HideFolderExplorerCommand { get; }
    public ICommand OpenFolderCommand { get; }

    private bool _isFolderExplorerVisible = false;
    public bool IsFolderExplorerVisible
    {
        get { return _isFolderExplorerVisible; }
        set { SetProperty(ref _isFolderExplorerVisible, value); }
    }

    private bool _searchAllTabs = false;
    public bool SearchAllTabs
    {
        get { return _searchAllTabs; }
        set
        {
            if (SetProperty(ref _searchAllTabs, value))
                OnPropertyChanged(nameof(SearchSelectedTab));
        }
    }


    public bool SearchSelectedTab
    {
        get { return !_searchAllTabs; }
        set { SearchAllTabs = !value; }
    }

    public AppViewModel()
    {
        FileOperations = new FileOperations(this);
        StatusBar = new StatusBarViewModel(this);


        NewCommand      = new RelayCommand(NewDocument);
        OpenCommand     = new RelayCommand(OpenDocument);
        SaveCommand     = new RelayCommand(SaveDocument, CanSave);
        SaveAsCommand   = new RelayCommand(SaveDocumentAs, CanSave);
        CloseCommand    = new RelayCommand<DocumentTab>(CloseDocument);
        CloseAllCommand = new RelayCommand(CloseAllDocuments, CanCloseAll);
        ExitCommand     = new RelayCommand(ExitApplication);
        AboutCommand    = new RelayCommand(OpenAbout);
        OpenFindCommand    = new RelayCommand(OpenFind);
        OpenReplaceCommand = new RelayCommand(OpenReplace);
        OpenReplaceAllCommand = new RelayCommand(OpenReplace);
        ToggleSearchAllTabsCommand = new RelayCommand(() => SearchAllTabs = !SearchAllTabs);
        ShowFolderExplorerCommand  = new RelayCommand(() => IsFolderExplorerVisible = true);
        HideFolderExplorerCommand  = new RelayCommand(() => IsFolderExplorerVisible = false);
        OpenFolderCommand          = new RelayCommand(OpenFolderAsWorkspace);


        FileOperations.NewDocument();
    }

    private void OpenFolderAsWorkspace()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog();
        dialog.Title = "Select Folder as Workspace";
        if (dialog.ShowDialog() == true)
        {


            string folderPath = dialog.FolderName;
            if (!folderPath.EndsWith("\\")) folderPath += "\\";

            Explorer.UpdatePath(folderPath + "temp.txt");
            IsFolderExplorerVisible = true;
        }
    }

    private void NewDocument()      => FileOperations.NewDocument();
    private void OpenDocument()     => FileOperations.OpenDocument();
    private void CloseAllDocuments()=> FileOperations.CloseAllDocuments();
    private void ExitApplication()
    {
        FileOperations.CloseAllDocuments();
        if (OpenTabs.Count == 0)
        {
            Application.Current.Shutdown();
        }
    }
    private bool CanCloseAll()      => OpenTabs.Count > 0;
    private bool CanSave()          => ActiveTab != null;

    private void SaveDocument()
    {
        if (ActiveTab != null)
            FileOperations.SaveDocument(ActiveTab);
    }

    private void SaveDocumentAs()
    {
        if (ActiveTab != null)
            FileOperations.SaveDocumentAs(ActiveTab);
    }

    private void CloseDocument(DocumentTab tab) => FileOperations.CloseDocument(tab);


    private void OpenAbout()
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.Owner = Application.Current.MainWindow;
        aboutWindow.ShowDialog();
    }


    private void OpenFind()
    {
        var main = Application.Current.MainWindow as MainWindow;
        if (main == null) return;
        var win = new MyNotepad.Features.Search.FindWindow(this, main);
        win.Owner = main;
        win.Show();
    }


    private void OpenReplace()
    {
        var main = Application.Current.MainWindow as MainWindow;
        if (main == null) return;
        var win = new MyNotepad.Features.Search.ReplaceWindow(this, main);
        win.Owner = main;
        win.Show();
    }
}
