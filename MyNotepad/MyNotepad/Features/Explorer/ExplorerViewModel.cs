using System;
using System.Collections.ObjectModel;
using System.IO;
using MyNotepad.Core;

namespace MyNotepad.Features.Explorer;

public class ExplorerViewModel : ObservableObject
{

    public ObservableCollection<ExplorerItemViewModel> Items { get; } = new ObservableCollection<ExplorerItemViewModel>();

    public ExplorerViewModel()
    {

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

            string rootPath = Path.GetPathRoot(filePath);
            if (string.IsNullOrEmpty(rootPath)) return;


            foreach (var drive in Items)
            {
                if (drive.FullPath.Equals(rootPath, StringComparison.OrdinalIgnoreCase))
                {

                    ExpandTo(drive, filePath);
                    break;
                }
            }
        }
        catch { }
    }

    private void ExpandTo(ExplorerItemViewModel currentNode, string targetPath)
    {

        currentNode.IsExpanded = true;


        foreach (var child in currentNode.Children)
        {

            if (child.FullPath.Equals(targetPath, StringComparison.OrdinalIgnoreCase))
            {
                child.IsSelected = true;
                return;
            }


            if (child.IsDirectory && targetPath.StartsWith(child.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                ExpandTo(child, targetPath);
                break;
            }
        }
    }
}
