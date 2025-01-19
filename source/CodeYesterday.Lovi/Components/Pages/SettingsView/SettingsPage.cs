using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components.Pages.SettingsView;

public class SettingsPage : ComponentBase
{
    [Parameter]
    public SettingsModel? Settings { get; set; }
}
