using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using System.Runtime.CompilerServices;

namespace CodeYesterday.Lovi.Importer.Clef;

[PublicAPI]
public class ClefImporter : IImporter
{
    public async IAsyncEnumerable<(LogEvent logEvent, long filePosition)> ImportLogsAsync(string filePath,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var clef = File.OpenText(filePath);

        var reader = new LogEventReader(clef);

        while (await reader.TryReadAsync(cancellationToken).ConfigureAwait(false) is { } evt)
        {
            yield return (evt, clef.BaseStream.Position);
        }
    }
}
