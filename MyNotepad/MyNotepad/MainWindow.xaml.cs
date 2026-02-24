using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace MyNotepad;



/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
    
    private void New_Click(object sender, RoutedEventArgs e)
    {
        textBox.Clear();
    }
    
    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            textBox.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
        
        saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
        saveFileDialog.DefaultExt = "txt";    
            
        if (saveFileDialog.ShowDialog() == true)
        {
            System.IO.File.WriteAllText(saveFileDialog.FileName, textBox.Text);
        }
    }

    public MainWindow()
    {
        InitializeComponent();
    }
}