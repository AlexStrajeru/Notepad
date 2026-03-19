using System;
using System.Collections.ObjectModel;
using System.IO;
using MyNotepad.Core;

namespace MyNotepad.Features.Explorer;

public class ExplorerViewModel : ObservableObject
{
    // Seteaza lista elementelor radacina afisate in arbore.
    public ObservableCollection<ExplorerItemViewModel> Items { get; } = new ObservableCollection<ExplorerItemViewModel>();

    public ExplorerViewModel()
    {
        // Incarcam toate discurile disponibile in calculator (C:\, D:\, G:\, etc.) la pornirea aplicatiei.
        try
        {
            foreach (var drive in Directory.GetLogicalDrives())
            {
                Items.Add(new ExplorerItemViewModel(drive, true));
            }
        }
        catch { }
    }

    public void UpdatePath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

        try
        {
            // Aflam discul pe care se afla fisierul (ex: C:\)
            string rootPath = Path.GetPathRoot(filePath);
            if (string.IsNullOrEmpty(rootPath)) return;

            // Cautam discul corect in lista noastra fara sa mai stergem nimic.
            foreach (var drive in Items)
            {
                if (drive.FullPath.Equals(rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Incepem expandarea de la acel disc spre fisier
                    ExpandTo(drive, filePath);
                    break;
                }
            }
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
            if (child.FullPath.Equals(targetPath, StringComparison.OrdinalIgnoreCase))
            {
                child.IsSelected = true;
                return;
            }

            // Suntem pe drumul cel bun (un folder parinte)? CONTINUAM
            if (child.IsDirectory && targetPath.StartsWith(child.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                ExpandTo(child, targetPath);
                break;
            }
        }
    }
}
