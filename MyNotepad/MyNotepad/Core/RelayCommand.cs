using System.Windows.Input;

namespace MyNotepad.Core;

// Leaga un buton sau meniu din UI de o metoda din cod.
// execute  = metoda care se ruleaza cand apesi butonul
// canExecute = metoda care decide daca butonul e activ sau gri (optional)
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    // WPF verifica automat daca butonul trebuie activat sau dezactivat
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    // Returneaza true daca butonul poate fi apasat
    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true; // fara restrictie = mereu activ
        return _canExecute();
    }

    // Ruleaza actiunea legata de buton
    public void Execute(object? parameter)
    {
        _execute();
    }
}

// Versiunea cu parametru — folosita de butonul X de pe tab (primeste tab-ul de inchis)
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true;
        if (parameter is T t) return _canExecute(t);
        return false;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T t) _execute(t);
    }
}
