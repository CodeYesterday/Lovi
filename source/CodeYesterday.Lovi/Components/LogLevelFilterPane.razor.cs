using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using CodeYesterday.Lovi.Session;
using Microsoft.AspNetCore.Components;
using Serilog.Events;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components;

public partial class LogLevelFilterPane
{
    [Parameter] public string? CssClass { get; set; }

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    [Inject]
    private AppModel Model { get; set; } = default!;

    private LogLevelFilterModel? LogLevelFilter => Model.Session?.LogLevelFilter;

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

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Model.SessionChanged += OnSessionChanged;

        if (Model.Session is null || LogLevelFilter is null) return;

        LogLevelFilter.FilterChanged += OnStateHasChanged;
    }

    private async Task OnExpandCollapseAllAsync(object? parameter)
    {
        bool expand = parameter is true;

        LogLevelFilter?.RootLayer.ExpandCollapseAll(expand);

        await InvokeAsync(StateHasChanged);
    }

    private void OnSessionChanged(object? sender, ChangedEventArgs<LogSession> e)
    {
        if (e.OldValue is not null)
        {
            e.OldValue.LogLevelFilter.FilterChanged -= OnStateHasChanged;
        }
        if (e.NewValue is not null)
        {
            e.NewValue.LogLevelFilter.FilterChanged += OnStateHasChanged;
        }
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