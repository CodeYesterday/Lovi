using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CodeYesterday.Lovi.Components;

public partial class StatusBarItemControl
{
    [Parameter]
    public StatusBarItem? Item { get; set; }

    [Inject]
    private TooltipService TooltipService { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    private void OnShowItemTooltip(ElementReference element, string? tooltip)
    {
        if (string.IsNullOrEmpty(tooltip)) return;

        TooltipService.Open(element, tooltip, SettingsService.GetSettings().TooltipOptions);
    }
}