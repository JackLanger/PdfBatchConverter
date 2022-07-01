using System;
using System.Windows.Input;

namespace PdfConverterWizard.controls;

public class RelayCommand : ICommand
{
    private Action _action;

    public RelayCommand(Action action)
    {
        this._action = action;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _action?.Invoke();

    public event EventHandler? CanExecuteChanged;
}