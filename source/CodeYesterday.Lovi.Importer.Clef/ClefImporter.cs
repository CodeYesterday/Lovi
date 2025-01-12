using CodeYesterday.Lovi.Models;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using System.Runtime.CompilerServices;

namespace CodeYesterday.Lovi.Importer.Clef;

[PublicAPI]
public class ClefImporter : IImporter
{
    public async IAsyncEnumerable<LogEvent> ImportLogsAsync(string filePath, Action<ProgressModel.ProgressData> progressCallback,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var clef = File.OpenText(filePath);

        progressCallback(new()
        {
            Max = clef.BaseStream.Length,
            Value = 0
        });

        var reader = new LogEventReader(clef);

        while (await reader.TryReadAsync(cancellationToken).ConfigureAwait(false) is { } evt)
        {
            yield return evt;
            progressCallback(new()
            {
                Max = clef.BaseStream.Length,
                Value = clef.BaseStream.Position
            });
        }
    }
}
