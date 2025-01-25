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

        LogItemModel CreatePreviewLogItem(int index, LogEventLevel level) =>
            new()
            {
                LogEvent = new LogEvent(DateTimeOffset.UtcNow.AddSeconds(index), level, null, new($"A {level} log event", []), []),
                FileId = 0,
                Id = index
            };

        PreviewLogItems = Settings.LogLevels.Select((l, i) => CreatePreviewLogItem(i, l)).ToArray();

        LogLevelFilterLayerModel CreatePreviewLogLevelFilterLayer(LogLevelFilterModel filter, bool? showValue) =>
            new(filter, null, null, 2)
            { ShowLayer = showValue, ShowFatal = showValue, ShowError = showValue, ShowWarning = showValue, ShowInformation = showValue, ShowDebug = showValue, ShowVerbose = showValue };

        var logLevelFilter = new LogLevelFilterModel();
        PreviewLogLevelFilterLayers =
            [
                CreatePreviewLogLevelFilterLayer(logLevelFilter, null),
                CreatePreviewLogLevelFilterLayer(logLevelFilter, false),
                CreatePreviewLogLevelFilterLayer(logLevelFilter, true)
            ];
    }

    private void ResetAllLogLevels()
    {
        foreach (var logLevel in Settings.LogLevels)
        {
            ResetLogLevel(logLevel);
        }
    }

    private void ResetLogLevel(LogEventLevel logLevel)
    {
        var logLevelSettings = Settings.GetLogLevelSettings(logLevel);
        var logLevelDefaultSettings = DefaultSettings.GetLogLevelSettings(logLevel);

        logLevelSettings.Icon = logLevelDefaultSettings.Icon;
        logLevelSettings.Color = logLevelDefaultSettings.Color;
        logLevelSettings.ContrastColor = logLevelDefaultSettings.ContrastColor;
    }
}