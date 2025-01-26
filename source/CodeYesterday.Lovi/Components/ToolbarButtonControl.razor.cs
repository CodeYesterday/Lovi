using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CodeYesterday.Lovi.Components;

public partial class ToolbarButtonControl
{
    public const Variant DefaultButtonVariant = Variant.Text;
    public const ButtonSize DefaultButtonSize = ButtonSize.Medium;
    public const ButtonStyle DefaultButtonStyle = ButtonStyle.Base;

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    [Parameter]
    public ToolbarButton? Button { get; set; }

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    [Inject]
    private TooltipService TooltipService { get; set; } = default!;

    private void OnShowTooltip(ElementReference element, string? tooltip)
    {
        if (string.IsNullOrEmpty(tooltip)) return;

        TooltipService.Open(element, tooltip, SettingsService.Settings.TooltipOptions);
    }
}