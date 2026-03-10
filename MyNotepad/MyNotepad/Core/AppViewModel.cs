using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyNotepad.Features.File;
using MyNotepad.Features.StatusBar;
using MyNotepad.Features.Help;

namespace MyNotepad.Core;

public class AppViewModel : ObservableObject
{
    // Lista de tab-uri deschise — UI-ul o afiseaza automat
    public ObservableCollection<DocumentTab> OpenTabs { get; } = new ObservableCollection<DocumentTab>();

    // Tab-ul selectat in momentul de fata
    private DocumentTab? _activeTab;
    public DocumentTab? ActiveTab
    {
        get { return _activeTab; }
        set { SetProperty(ref _activeTab, value); }
    }

    public int DocumentCounter { get; set; } = 1;

    // Clasa care se ocupa cu operatiile pe fisiere (new, open, save, close)
    public FileOperations FileOperations { get; }

    // Datele afisate in bara de jos (linie, coloana, status)
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

    // Inversul lui SearchAllTabs — cele doua functioneaza ca radio buttons
    public bool SearchSelectedTab
    {
        get { return !_searchAllTabs; }
        set { SearchAllTabs = !value; }
    }

    public AppViewModel()
    {
        FileOperations = new FileOperations(this);
        StatusBar = new StatusBarViewModel(this);

        // Leaga fiecare comanda de metoda corespunzatoare
        NewCommand      = new RelayCommand(NewDocument);
        OpenCommand     = new RelayCommand(OpenDocument);
        SaveCommand     = new RelayCommand(SaveDocument, CanSave);       // CanSave = activ doar daca e un tab deschis
        SaveAsCommand   = new RelayCommand(SaveDocumentAs, CanSave);
        CloseCommand    = new RelayCommand<DocumentTab>(CloseDocument);  // primeste ca parametru tab-ul de inchis
        CloseAllCommand = new RelayCommand(CloseAllDocuments, CanCloseAll);
        ExitCommand     = new RelayCommand(ExitApplication);
        AboutCommand    = new RelayCommand(OpenAbout);
        OpenFindCommand    = new RelayCommand(OpenFind);
        OpenReplaceCommand = new RelayCommand(OpenReplace);
        OpenReplaceAllCommand = new RelayCommand(OpenReplace);
        ToggleSearchAllTabsCommand = new RelayCommand(() => SearchAllTabs = !SearchAllTabs);

        // Deschide un tab gol la pornirea aplicatiei
        FileOperations.NewDocument();
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

    // Deschide fereastra About (informatii despre aplicatie)
    private void OpenAbout()
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.Owner = Application.Current.MainWindow;
        aboutWindow.ShowDialog();
    }

    // Deschide fereastra de cautare text
    private void OpenFind()
    {
        var main = Application.Current.MainWindow as MainWindow;
        if (main == null) return;
        var win = new MyNotepad.Features.Search.FindWindow(this, main);
        win.Owner = main;
        win.Show();
    }

    // Deschide fereastra de inlocuire text
    private void OpenReplace()
    {
        var main = Application.Current.MainWindow as MainWindow;
        if (main == null) return;
        var win = new MyNotepad.Features.Search.ReplaceWindow(this, main);
        win.Owner = main;
        win.Show();
    }
}
