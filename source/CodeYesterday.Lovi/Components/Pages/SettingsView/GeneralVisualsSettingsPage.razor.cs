using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CodeYesterday.Lovi.Components.Pages.SettingsView;

public partial class GeneralVisualsSettingsPage
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ThemeService ThemeService { get; set; } = default!;

    [Inject]
    private IViewManager ViewManager { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(ThemeService.Theme))
        {
            ThemeService.SetTheme(Settings.Theme, false);
        }
        return base.OnInitializedAsync();
    }

    private void ChangeTheme(string newTheme)
    {
        Settings.Theme = newTheme;
        ThemeService.SetTheme(newTheme, false);

        _ = ViewManager.Refresh(CancellationToken.None);
    }
}