namespace CodeYesterday.Lovi.Services;

internal class MruService(IUserSettingsService<MruService> userSettings) : IMruService
{
    private const int MaxEntryCount = 10;

    private readonly IUserSettingsService<MruService> _userSettings = userSettings;

    public IEnumerable<string> GetMruSessionDirectories()
    {
        return ReadMruEntries();
    }

    public void SetMruSessionDirectory(string sessionDirectory)
    {
        var sessionDirectories = ReadMruEntries();

        // TODO: handle case sensitivity platform specific.
        var index = sessionDirectories.FindIndex(d => string.Equals(d, sessionDirectory, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            sessionDirectories.RemoveAt(index);
        }

        if (sessionDirectories.Any())
        {
            sessionDirectories.Insert(0, sessionDirectory);
        }
        else
        {
            sessionDirectories.Add(sessionDirectory);
        }

        while (sessionDirectories.Count > MaxEntryCount)
        {
            sessionDirectories.RemoveAt(sessionDirectories.Count - 1);
        }

        WriteMruEntries(sessionDirectories);
    }

    private List<string> ReadMruEntries()
    {
        var sessionDirectories = new List<string>();
        for (int n = 0; n < MaxEntryCount; ++n)
        {
            if (_userSettings.TryGetValue($"MRU{n}", out string? sessionDirectory))
            {
                try
                {
                    if (!string.IsNullOrEmpty(sessionDirectory) && Directory.Exists(sessionDirectory))
                    {
                        sessionDirectories.Add(sessionDirectory);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
        return sessionDirectories;
    }

    private void WriteMruEntries(List<string> sessionDirectories)
    {
        for (int n = 0; n < MaxEntryCount; ++n)
        {
            var id = $"MRU{n}";
            if (sessionDirectories.Count > n)
            {
                _userSettings.SetValue(id, sessionDirectories[n]);
            }
            else
            {
                _userSettings.RemoveValue(id);
            }
        }
    }
}
