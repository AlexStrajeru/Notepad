using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MyNotepad.Commands;

namespace MyNotepad.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public ICommand NewCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ExitCommand { get; }

    public MainViewModel()
    {
        NewCommand  = new RelayCommand(_ => Text = string.Empty);
        OpenCommand = new RelayCommand(_ => Open());
        SaveCommand = new RelayCommand(_ => Save());
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
    }

    private void Open()
    {
        var openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
            Text = File.ReadAllText(openFileDialog.FileName);
    }

    private void Save()
    {
        var saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
        saveFileDialog.DefaultExt = "txt";
        if (saveFileDialog.ShowDialog() == true)
            File.WriteAllText(saveFileDialog.FileName, Text);
    }
}
