using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components;

public partial class ToolbarControl
{
    [Parameter]
    public Toolbar? Toolbar { get; set; }

    [Inject]
    private TooltipService TooltipService { get; set; } = default!;

    [Inject]
    private ContextMenuService ContextMenuService { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    private void OnShowToolbarItemTooltip(ElementReference element, string? tooltip)
    {
        if (string.IsNullOrEmpty(tooltip)) return;

        TooltipService.Open(element, tooltip, SettingsService.Settings.TooltipOptions);
    }

    private void OpenButtonMenu(MouseEventArgs args, ToolbarMenuButton menuButton)
    {
        if (menuButton.Items.All(mb => !mb.IsVisible)) return;

        ContextMenuService.Open(args,
            menuButton.Items
                .Where(mb => mb.IsVisible)
                .Select(mb => new ContextMenuItem
                {
                    Icon = mb.Icon,
                    Text = mb.Title,
                    Disabled = !mb.Command.CanExecute(mb.CommandParameter),
                    Value = mb
                }),
            a =>
            {
                ((ToolbarButton)a.Value).Command.Execute(((ToolbarButton)a.Value).CommandParameter);
                ContextMenuService.Close();
            });
    }
}