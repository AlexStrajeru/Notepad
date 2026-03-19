using System.Collections.ObjectModel;
using System.IO;
using MyNotepad.Core;

namespace MyNotepad.Features.Explorer;

public class ExplorerViewModel : ObservableObject
{
    // Seteaza lista elementelor radacina afisate in arbore.
    public ObservableCollection<ExplorerItemViewModel> Items { get; } = new ObservableCollection<ExplorerItemViewModel>();

    public void UpdatePath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

        try
        {
            // Pornim de la radacina discului (ex: C:\)
            string rootPath = Path.GetPathRoot(filePath) ?? filePath;

            if (Items.Count == 0 || !Items[0].FullPath.Equals(rootPath, System.StringComparison.OrdinalIgnoreCase))
            {
                Items.Clear();
                var driveRoot = new ExplorerItemViewModel(rootPath, true);
                Items.Add(driveRoot);
            }

            // Incepem expandarea de la radacina spre fisier
            ExpandTo(Items[0], filePath);
        }
        catch { }
    }

    private void ExpandTo(ExplorerItemViewModel currentNode, string targetPath)
    {
        // 1. Deschidem folderul curent
        currentNode.IsExpanded = true; 

        // 2. Itearam prin copii (foldere sau fisiere)
        foreach (var child in currentNode.Children)
        {
            // Am gasit exact fisierul? IL SELECTAM
            if (child.FullPath.Equals(targetPath, System.StringComparison.OrdinalIgnoreCase))
            {
                child.IsSelected = true;
                return;
            }

            // Suntem pe drumul cel bun (un folder parinte)? CONTINUAM
            if (child.IsDirectory && targetPath.StartsWith(child.FullPath, System.StringComparison.OrdinalIgnoreCase))
            {
                ExpandTo(child, targetPath);
                break;
            }
        }
    }
}
