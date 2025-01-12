using System.Windows.Input;

namespace CodeYesterday.Lovi.Input;

internal class AsyncCommand : ICommand
{
    private readonly ExecuteAsyncDelegate _executeCallback;
    private readonly CanExecuteDelegate? _canExecuteCallback;

    public delegate Task ExecuteAsyncDelegate(object? parameter);
    public delegate bool CanExecuteDelegate(object? parameter);

    public AsyncCommand(ExecuteAsyncDelegate executeCallback, CanExecuteDelegate? canExecuteCallback = null)
    {
        _executeCallback = executeCallback;
        _canExecuteCallback = canExecuteCallback;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecuteCallback?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        // Fire and forget, exceptions will be swallowed into the void.
        _ = _executeCallback(parameter);
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? CanExecuteChanged;
}
