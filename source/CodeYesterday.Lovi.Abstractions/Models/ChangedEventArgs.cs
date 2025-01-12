namespace CodeYesterday.Lovi.Models;

public class ChangedEventArgs<T>(T? oldValue, T? newValue) : EventArgs
{
    public T? OldValue { get; } = oldValue;

    public T? NewValue { get; } = newValue;
}
