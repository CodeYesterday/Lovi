namespace CodeYesterday.Lovi.Models;

public class ExceptionEventArgs(Exception exception) : EventArgs
{
    public Exception Exception { get; } = exception;
}
