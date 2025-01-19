using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace CodeYesterday.Lovi.Components;

public partial class ResetSettingButton
{
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    [Parameter]
    public EventCallback<MouseEventArgs> Click { get; set; }

    [Parameter]
    public string? SettingName { get; set; } = string.Empty;

    [Parameter]
    public object? DefaultValue { get; set; }

    [Inject]
    private TooltipService TooltipService { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    private Task OnShowTooltip(ElementReference e)
    {
        if (!string.IsNullOrEmpty(SettingName))
        {
            var tooltip = DefaultValue is null
                ? $"Reset {SettingName} to default"
                : $"Reset {SettingName} to default ({DefaultValue})";

            TooltipService.Open(e, tooltip, SettingsService.Settings.TooltipOptions);
        }

        return Task.CompletedTask;
    }
}