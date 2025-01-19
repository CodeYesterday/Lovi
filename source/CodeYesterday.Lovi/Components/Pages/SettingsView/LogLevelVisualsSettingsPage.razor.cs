using CodeYesterday.Lovi.Models;
using Serilog.Events;

namespace CodeYesterday.Lovi.Components.Pages.SettingsView;

public partial class LogLevelVisualsSettingsPage
{
    private LogItemModel[] PreviewLogItems { get; set; } = [];

    private LogLevelFilterLayerModel[] PreviewLogLevelFilterLayers { get; set; } = [];

    private IList<LogItemModel>? SelectedItems { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        PreviewLogItems =
        [
            new LogItemModel
            {
                LogEvent = new LogEvent(DateTimeOffset.UtcNow.AddSeconds(-5), LogEventLevel.Verbose, null, new("A Verbose log event", []), []),
                FileId = 0,
                Id = 0
            },
            new LogItemModel
            {
                LogEvent = new LogEvent(DateTimeOffset.UtcNow.AddSeconds(-4), LogEventLevel.Debug, null, new("A Debug log event", []), []),
                FileId = 0,
                Id = 1
            },
            new LogItemModel
            {
                LogEvent = new LogEvent(DateTimeOffset.UtcNow.AddSeconds(-3), LogEventLevel.Information, null, new("A Information log event", []), []),
                FileId = 0,
                Id = 2
            },
            new LogItemModel
            {
                LogEvent = new LogEvent(DateTimeOffset.UtcNow.AddSeconds(-2), LogEventLevel.Warning, null, new("A Warning log event", []), []),
                FileId = 0,
                Id = 3
            },
            new LogItemModel
            {
                LogEvent = new LogEvent(DateTimeOffset.UtcNow.AddSeconds(-1), LogEventLevel.Error, null, new("A Error log event", []), []),
                FileId = 0,
                Id = 4
            },
            new LogItemModel
            {
                LogEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Fatal, null, new("A Fatal log event", []), []),
                FileId = 0,
                Id = 5
            }
        ];

        var logLevelFilter = new LogLevelFilterModel();
        PreviewLogLevelFilterLayers =
            [
                new(logLevelFilter, null, null, 2),
                new(logLevelFilter, null, null, 2)
                {
                    ShowLayer = false, ShowFatal = false, ShowError = false, ShowWarning = false, ShowInformation = false, ShowDebug = false, ShowVerbose = false
                },
                new(logLevelFilter, null, null, 2)
                {
                    ShowLayer = true, ShowFatal = true, ShowError = true, ShowWarning = true, ShowInformation = true, ShowDebug = true, ShowVerbose = true
                }
            ];
    }

    private void ResetAllLogLevels()
    {
        if (Settings is null) return;

        foreach (var logLevel in Settings.LogLevels)
        {
            ResetLogLevel(logLevel);
        }
    }

    private void ResetLogLevel(LogEventLevel logLevel)
    {
        if (Settings is null) return;

        var logLevelSettings = Settings.GetLogLevelSettings(logLevel);
        var logLevelDefaultSettings = DefaultSettings.GetLogLevelSettings(logLevel);

        logLevelSettings.Icon = logLevelDefaultSettings.Icon;
        logLevelSettings.Color = logLevelDefaultSettings.Color;
        logLevelSettings.ContrastColor = logLevelDefaultSettings.ContrastColor;
    }
}