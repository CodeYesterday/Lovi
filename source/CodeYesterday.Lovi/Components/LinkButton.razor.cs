using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CodeYesterday.Lovi.Components;

public partial class LinkButton
{
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    [Parameter]
    public string Uri { get; set; } = string.Empty;

    [Parameter]
    public string? Text { get; set; }

    [Inject]
    private TooltipService TooltipService { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    private Task OpenUri()
    {
        return Browser.Default.OpenAsync(Uri, BrowserLaunchMode.SystemPreferred);
    }

    private void OpenTooltip(ElementReference e)
    {
        TooltipService.Open(e, Uri, SettingsService.Settings.TooltipOptions);
    }
}