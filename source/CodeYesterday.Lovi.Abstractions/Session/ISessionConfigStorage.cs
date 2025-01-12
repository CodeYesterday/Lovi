namespace CodeYesterday.Lovi.Session;

public interface ISessionConfigStorage
{
    Task<SessionConfig> ReadSessionConfigAsync(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken);

    Task WriteSessionConfigAsync(SessionConfig data, string sessionDirectory, string dataDirectory, CancellationToken cancellationToken);
}