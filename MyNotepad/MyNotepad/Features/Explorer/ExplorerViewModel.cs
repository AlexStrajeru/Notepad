using System.Collections.ObjectModel;
using System.IO;
using MyNotepad.Core;

namespace MyNotepad.Features.Explorer;

public class ExplorerViewModel : ObservableObject
{
    // Seteaza lista elementelor radacina afisate in arbore.
    public ObservableCollection<ExplorerItemViewModel> Items { get; } = new ObservableCollection<ExplorerItemViewModel>();

    // Reincarca structura panoului in functie de locatia fisierului curent.
    public void UpdatePath(string? filePath)
    {
        Items.Clear();

        // Lasa structura goala daca fisierul nu este inca salvat pe disc.
        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            // Extrage structura bazata pe fisier si verifica existenta acesteia utilizand clase native din libraria System.IO.
            string? dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

            // Selecteaza folderul parinte al fisierului ca nod principal si il extinde automat.
            var root = new ExplorerItemViewModel(dir, true);
            root.IsExpanded = true; 
            root.LoadChildren();
            
            Items.Add(root);
        }
        catch
        {
            // Abandoneaza operatia in caz ca apar erori de permisiuni.
        }
    }
}
