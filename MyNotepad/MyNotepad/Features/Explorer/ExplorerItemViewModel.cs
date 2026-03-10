using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MyNotepad.Core;

namespace MyNotepad.Features.Explorer;

public class ExplorerItemViewModel : ObservableObject
{
    private bool _isExpanded;
    private bool _isLoaded;

    public string Name { get; }
    public string FullPath { get; }
    public bool IsDirectory { get; }
    
    // Stabileste iconita afisata in functie de tipul elementului.
    public string Icon => IsDirectory ? "📁" : "📄";

    public ObservableCollection<ExplorerItemViewModel> Children { get; } = new ObservableCollection<ExplorerItemViewModel>();

    // Creeaza un element temporar folosit inainte de incarcarea datelor reale.
    private ExplorerItemViewModel(string dummyName)
    {
        Name = dummyName;
        FullPath = "";
        IsDirectory = false;
    }

    public ExplorerItemViewModel(string fullPath, bool isDirectory)
    {
        FullPath = fullPath;
        IsDirectory = isDirectory;
        Name = Path.GetFileName(fullPath);
        if (string.IsNullOrEmpty(Name))
            Name = fullPath; // Foloseste calea completa daca elementul este o partitie din sistem sau o cale principala.

        if (IsDirectory)
        {
            // Adauga un element temporar pentru a forta afisarea sagetii de expandare in interfata.
            Children.Add(new ExplorerItemViewModel("Loading..."));
        }
    }

    // Incarca asincron continutul directorului doar in momentul in care utilizatorul il extinde manual.
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value))
            {
                if (_isExpanded && !_isLoaded && IsDirectory)
                {
                    LoadChildren();
                }
            }
        }
    }

    public void LoadChildren()
    {
        Children.Clear();
        if (!IsDirectory) return;

        try
        {
            // Extrage fisierele si folderele pe baza caii utilizand clasa Directory din libraria System.IO.
            var dirs = Directory.GetDirectories(FullPath).OrderBy(d => d).ToList();
            var files = Directory.GetFiles(FullPath).OrderBy(f => f).ToList();

            foreach (var dir in dirs)
                Children.Add(new ExplorerItemViewModel(dir, true));

            foreach (var file in files)
                Children.Add(new ExplorerItemViewModel(file, false));

            _isLoaded = true;
        }
        catch (Exception)
        {
            // Ignora directoarele care necesita drepturi de administrator.
        }
    }
}
