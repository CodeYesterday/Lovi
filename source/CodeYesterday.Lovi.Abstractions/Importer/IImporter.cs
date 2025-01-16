using Serilog.Events;

namespace CodeYesterday.Lovi.Importer;

[PublicAPI]
public interface IImporter
{
    IAsyncEnumerable<(LogEvent logEvent, long filePosition)> ImportLogsAsync(string filePath, CancellationToken cancellationToken);
}
