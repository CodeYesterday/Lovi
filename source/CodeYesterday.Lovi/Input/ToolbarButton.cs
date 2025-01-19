using Radzen;
using System.Windows.Input;

namespace CodeYesterday.Lovi.Input;

[PublicAPI]
public class ToolbarButton : ToolbarItem
{
    public required ICommand Command { get; init; }

    public object? CommandParameter { get; init; }

    public ButtonStyle? ButtonStyle { get; init; }

    public Variant? ButtonVariant { get; init; }
}
