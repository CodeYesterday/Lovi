using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;
using Serilog.Events;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components;

public partial class LogLevelFilterPane
{
    [Parameter] public string? CssClass { get; set; }

    private LogLevelFilterModel? LogLevelFilter => Session?.LogLevelFilter;

    private Toolbar Toolbar { get; }

    public LogLevelFilterPane()
    {
        Toolbar = new()
        {
            Id = "local",
            Items =
            [
                new ToolbarButton
                {
                    Icon = "expand_all",
                    Tooltip = "Expand all",
                    Command = new AsyncCommand(OnExpandCollapseAllAsync),
                    CommandParameter = true
                },
                new ToolbarButton
                {
                    Icon = "collapse_all",
                    Tooltip = "Collapse all",
                    Command = new AsyncCommand(OnExpandCollapseAllAsync),
                    CommandParameter = false
                }
            ]
        };
    }

    public override async Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        await base.OnOpeningAsync(cancellationToken);

        if (LogLevelFilter is null) return;

        LogLevelFilter.FilterChanged += OnStateHasChanged;
    }

    public override Task OnClosingAsync(CancellationToken cancellationToken)
    {
        if (LogLevelFilter is not null)
        {
            LogLevelFilter.FilterChanged -= OnStateHasChanged;
        }

        return base.OnClosingAsync(cancellationToken);
    }

    private async Task OnExpandCollapseAllAsync(object? parameter)
    {
        bool expand = parameter is true;

        LogLevelFilter?.RootLayer.ExpandCollapseAll(expand);

        await InvokeAsync(StateHasChanged);
    }

    private void OnStateHasChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private string GetCheckBoxColorStyles(LogEventLevel logLevel)
    {
        var logLevelSettings = SettingsService.GetSettings().GetLogLevelSettings(logLevel);

        return
            $"--rz-checkbox-checked-background-color: {logLevelSettings.Color}; " +
            $"--rz-input-background-color: {logLevelSettings.Color}; " +
            $"--rz-checkbox-checked-hover-background-color: color-mix(in srgb, {logLevelSettings.Color}, white 20%);" +
            (logLevelSettings.ContrastColor is null ? string.Empty : $" --rz-checkbox-checked-color: {logLevelSettings.ContrastColor};");
    }
}