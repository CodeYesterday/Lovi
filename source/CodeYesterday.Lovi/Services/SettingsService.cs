using CodeYesterday.Lovi.Models;
using Serilog.Events;

namespace CodeYesterday.Lovi.Services;

internal class SettingsService : ISettingsService
{
    private readonly IUserSettingsService<SettingsService> _userSettingsService;
    public SettingsModel Settings { get; } = new();

    public SettingsService(IUserSettingsService<SettingsService> userSettingsService)
    {
        _userSettingsService = userSettingsService;
        LoadSettings();
    }

    public Task LoadSettingsAsync(CancellationToken cancellationToken)
    {
        LoadSettings();

        return Task.CompletedTask;
    }

    public Task SaveSettingsAsync(CancellationToken cancellationToken)
    {
        _userSettingsService.SetValue("TimestampFormat", Settings.TimestampFormat);

        foreach (var level in Settings.LogLevels)
        {
            SaveLogLevelSettings(level);
        }

        return Task.CompletedTask;
    }

    private void LoadSettings()
    {
        Settings.TimestampFormat = _userSettingsService.GetValue("TimestampFormat", "yyyy-MM-dd HH:mm:ss.fff");

        LoadLogLevelSettings(LogEventLevel.Verbose, "density_small", "lightsalmon", "black");
        LoadLogLevelSettings(LogEventLevel.Debug, "adb", "darkgray", "black");
        LoadLogLevelSettings(LogEventLevel.Information, "info", "steelblue");
        LoadLogLevelSettings(LogEventLevel.Warning, "warning", "orange", "black");
        LoadLogLevelSettings(LogEventLevel.Error, "error", "lightsalmon", "black");
        LoadLogLevelSettings(LogEventLevel.Fatal, "crisis_alert", "mediumvioletred", "black");
    }

    private void LoadLogLevelSettings(LogEventLevel level, string icon, string color, string? contrastColor = null)
    {
        var levelSettings = Settings.GetLogLevelSettings(level);

        levelSettings.Icon = _userSettingsService.GetValue($"{level}.Icon", icon);
        levelSettings.Color = _userSettingsService.GetValue($"{level}.Color", color);
        levelSettings.ContrastColor = _userSettingsService.GetValue($"{level}.ContrastColor", contrastColor ?? string.Empty);
        if (string.IsNullOrEmpty(levelSettings.ContrastColor))
        {
            levelSettings.ContrastColor = null;
        }
    }

    private void SaveLogLevelSettings(LogEventLevel level)
    {
        var levelSettings = Settings.GetLogLevelSettings(level);

        _userSettingsService.SetValue($"{level}.Icon", levelSettings.Icon);
        _userSettingsService.SetValue($"{level}.Color", levelSettings.Color);
        _userSettingsService.SetValue($"{level}.ContrastColor", levelSettings.ContrastColor ?? string.Empty);
    }
}
