using CodeYesterday.Lovi.Input;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components;

public partial class AppToolbar
{
    [CascadingParameter]
    public ToolbarContainer ToolbarContainer { get; set; } = default!;
}