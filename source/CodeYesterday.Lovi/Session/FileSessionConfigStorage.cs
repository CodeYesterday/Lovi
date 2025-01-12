using System.Text.Json;

namespace CodeYesterday.Lovi.Session;

internal class FileSessionConfigStorage : ISessionConfigStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<SessionConfig> ReadSessionConfigAsync(string sessionDirectory, string dataDirectory,
        CancellationToken cancellationToken)
    {
        var path = GetSessionFilePath(dataDirectory);
        await using var fileStream = File.OpenRead(path);

        var data = await JsonSerializer.DeserializeAsync<SessionConfig>(fileStream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (data is null) throw new InvalidOperationException("Could not read session config JSON file.");

        return data;
    }

    public async Task WriteSessionConfigAsync(SessionConfig data, string sessionDirectory, string dataDirectory,
        CancellationToken cancellationToken)
    {
        var path = GetSessionFilePath(dataDirectory);
        await using var fileStream = File.Create(path);

        await JsonSerializer.SerializeAsync(fileStream, data, JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private static string GetSessionFilePath(string dataDirectory)
    {
        return Path.Combine(dataDirectory, "SessionConfig.json");
    }
}