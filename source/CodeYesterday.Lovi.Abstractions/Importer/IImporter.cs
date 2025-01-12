using CodeYesterday.Lovi.Models;
using Serilog.Events;

namespace CodeYesterday.Lovi.Importer;

[PublicAPI]
public interface IImporter
{
    IAsyncEnumerable<LogEvent> ImportLogsAsync(string filePath, Action<ProgressModel.ProgressData> progressCallback,
        CancellationToken cancellationToken);
}
