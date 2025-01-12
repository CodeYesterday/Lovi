using CodeYesterday.Lovi.Models;

namespace CodeYesterday.Lovi.Services;

internal class SettingsService : ISettingsService
{
    private SettingsModel _settings = new();

    public SettingsModel GetSettings()
    {
        return _settings;
    }
}
