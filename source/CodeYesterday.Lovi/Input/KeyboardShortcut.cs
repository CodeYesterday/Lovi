using System.Windows.Input;

namespace CodeYesterday.Lovi.Input;

public class KeyboardShortcut
{
    public required string Id { get; init; }

    public required ICommand Command { get; init; }

    public object? CommandParameter { get; init; }

    public required string KeyCode { get; init; }

    public bool ShiftKey { get; init; }

    public bool AltKey { get; init; }

    public bool CtrlKey { get; init; }
}