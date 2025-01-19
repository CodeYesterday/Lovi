using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Diagnostics;
using System.Text.Json;

namespace CodeYesterday.Lovi.Components;

public partial class AboutDialog
{
    public static Task ShowAsync(DialogService dialogService)
    {
        return dialogService.OpenAsync<AboutDialog>("About", options: new()
        {
            Width = "500px",
            Height = "700px"
        });
    }

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    [Inject]
    private TooltipService TooltipService { get; set; } = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    private string Version { get; set; } = string.Empty;

    private List<PackageLicenseModel> Licenses { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        // Get the file version from Lovi exe
        try
        {
            Version = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).FileVersion ?? "?";
        }
        catch (Exception ex)
        {
            Version = "?";
            NotificationService.Notify(NotificationSeverity.Error, "Get file version failed", ex.Message, 20000d);
        }

        // Read 3rd party licenses from embedded resource file:
        try
        {
            await using var stream = GetType().Assembly.GetManifestResourceStream("CodeYesterday.Lovi.licenses.json");
            if (stream is not null)
            {
                Licenses =
                    await JsonSerializer.DeserializeAsync<List<PackageLicenseModel>>(stream).ConfigureAwait(true) ?? [];
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Read licenses.json failed", ex.Message, 20000d);
        }

        await base.OnInitializedAsync().ConfigureAwait(true);
    }

    private async Task OpenUrl(string url)
    {
        try
        {
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Open URL failed", ex.Message, 20000d);
        }
    }

    private void ShowTooltip(ElementReference element, string? tooltip)
    {
        if (string.IsNullOrEmpty(tooltip)) return;

        TooltipService.Open(element, tooltip, SettingsService.Settings.TooltipOptions);
    }
}