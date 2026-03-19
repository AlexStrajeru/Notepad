using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyNotepad.Features.File;
using MyNotepad.Features.StatusBar;
using MyNotepad.Features.Help;

namespace MyNotepad.Core;

public class AppViewModel : ObservableObject
{
    // Contine lista documentelor deschise curent in aplicatie.
    public ObservableCollection<DocumentTab> OpenTabs { get; } = new ObservableCollection<DocumentTab>();

    // Returneaza sau seteaza documentul activ in mod curent.
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

    // Gestioneaza operatiunile pe fisiere.
    public FileOperations FileOperations { get; }

    // Modelul de date pentru interfata de afisare a directoarelor.
    public Features.Explorer.ExplorerViewModel Explorer { get; } = new Features.Explorer.ExplorerViewModel();

    // Contine informatiile afisate in bara de stare a aplicatiei.
    public StatusBarViewModel StatusBar { get; }

    // Comenzile legate de butoane si meniuri
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
    public ICommand OpenFolderCommand { get; } // Comanda adaugata din nou

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

    // Proprietatea inversa celei principale pentru sincronizarea elementelor de interfata.
    public bool SearchSelectedTab
    {
        get { return !_searchAllTabs; }
        set { SearchAllTabs = !value; }
    }

    public AppViewModel()
    {
        FileOperations = new FileOperations(this);
        StatusBar = new StatusBarViewModel(this);

        // Initializeaza comenzile din interfata aplicatiei.
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
        OpenFolderCommand          = new RelayCommand(OpenFolderAsWorkspace); // Linkare comanda

        // Creeaza un prim document gol odata cu rularea aplicatiei.
        FileOperations.NewDocument();
    }

    private void OpenFolderAsWorkspace()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog();
        dialog.Title = "Select Folder as Workspace";
        if (dialog.ShowDialog() == true)
        {
            // Folosim UpdatePath pentru a deschide automat ierarhia pana la folderul ales.
            // Punem un fisier fictiv 'temp' la final ca sa pacalim functia sa gaseasca folderul.
            string folderPath = dialog.FolderName;
            if (!folderPath.EndsWith("\\")) folderPath += "\\";
            
            Explorer.UpdatePath(folderPath + "temp.txt"); 
            IsFolderExplorerVisible = true;
        }
    }

    private void NewDocument()      => FileOperations.NewDocument();
    private void OpenDocument()     => FileOperations.OpenDocument();
    private void CloseAllDocuments()=> FileOperations.CloseAllDocuments();
    private void ExitApplication()  => Application.Current.Shutdown();
    private bool CanCloseAll()      => OpenTabs.Count > 0;
    private bool CanSave()          => ActiveTab != null; // dezactiveaza Save daca nu e niciun tab

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

    // Deschide fereastra cu informatii despre student.
    private void OpenAbout()
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.Owner = Application.Current.MainWindow;
        aboutWindow.ShowDialog();
    }

    // Afiseaza fereastra pentru cautarea textului.
    private void OpenFind()
    {
        var main = Application.Current.MainWindow as MainWindow;
        if (main == null) return;
        var win = new MyNotepad.Features.Search.FindWindow(this, main);
        win.Owner = main;
        win.Show();
    }

    // Afiseaza fereastra pentru inlocuirea textului.
    private void OpenReplace()
    {
        var main = Application.Current.MainWindow as MainWindow;
        if (main == null) return;
        var win = new MyNotepad.Features.Search.ReplaceWindow(this, main);
        win.Owner = main;
        win.Show();
    }
}
