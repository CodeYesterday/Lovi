using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components.Pages.SettingsView;

public class SettingsPage : ComponentBase
{
    [Parameter]
    public SettingsModel? Settings { get; set; }

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    protected SettingsModel DefaultSettings => SettingsService.DefaultSettings;
}
