using CodeYesterday.Lovi.Models;

namespace CodeYesterday.Lovi.Services;

public interface ISettingsService
{
    SettingsModel Settings { get; }

    Task LoadSettingsAsync(CancellationToken cancellationToken);

    Task SaveSettingsAsync(CancellationToken cancellationToken);
}
