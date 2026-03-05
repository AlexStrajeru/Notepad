using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyNotepad.Core;

// Clasa de baza pentru orice obiect ale carui proprietati sunt afisate in UI.
// Cand o proprietate se schimba, UI-ul se actualizeaza automat.
public abstract class ObservableObject : INotifyPropertyChanged
{
    // Evenimentul pe care WPF il asculta pentru a sti cand sa redeseneze UI-ul
    public event PropertyChangedEventHandler? PropertyChanged;

    // Anunta UI-ul ca proprietatea cu numele dat s-a schimbat
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Seteaza valoarea unui camp si anunta UI-ul — returneaza true daca valoarea s-a schimbat
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false; // valoarea e aceeasi, nu face nimic

        field = value;
        OnPropertyChanged(propertyName); // anunta UI-ul sa se actualizeze
        return true;
    }
}
