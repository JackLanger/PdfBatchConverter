using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace PdfBatchConverterWizzard.controls;

public class ParamCommand<T> : ICommand
{
    
    private Action<T> _action;

    public ParamCommand(Action<T> action)
    {
        this._action = action;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute([NotNull]object? parameter) => _action?.Invoke((T) parameter);

    public event EventHandler? CanExecuteChanged;
}