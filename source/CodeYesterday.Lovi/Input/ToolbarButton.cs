using System.Windows.Input;

namespace CodeYesterday.Lovi.Input;

public class ToolbarButton : ToolbarItem
{
    public required ICommand Command { get; init; }

    public object? CommandParameter { get; init; }
}
