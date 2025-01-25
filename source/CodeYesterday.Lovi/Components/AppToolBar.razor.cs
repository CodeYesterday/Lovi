using CodeYesterday.Lovi.Input;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components;

public partial class AppToolbar
{
    [CascadingParameter]
    public InputHandler ToolbarContainer { get; set; } = default!;
}