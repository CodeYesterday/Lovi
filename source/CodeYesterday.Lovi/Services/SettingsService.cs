using CodeYesterday.Lovi.Models;
using Serilog.Events;

namespace CodeYesterday.Lovi.Services;

internal class SettingsService : ISettingsService
{
    private readonly IUserSettingsService<SettingsService> _userSettingsService;

    public SettingsModel Settings { get; } = new();

    public SettingsModel DefaultSettings { get; } = new();

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
        Settings.TimestampFormat = _userSettingsService.GetValue("TimestampFormat", DefaultSettings.TimestampFormat);

        foreach (var level in Settings.LogLevels)
        {
            LoadLogLevelSettings(level);
        }
    }

    private void LoadLogLevelSettings(LogEventLevel level)
    {
        var levelSettings = Settings.GetLogLevelSettings(level);
        var defaultLevelSettings = DefaultSettings.GetLogLevelSettings(level);

        levelSettings.Icon = _userSettingsService.GetValue($"{level}.Icon", defaultLevelSettings.Icon);
        levelSettings.Color = _userSettingsService.GetValue($"{level}.Color", defaultLevelSettings.Color);
        levelSettings.ContrastColor = _userSettingsService.GetValue($"{level}.ContrastColor", defaultLevelSettings.ContrastColor ?? string.Empty);
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
