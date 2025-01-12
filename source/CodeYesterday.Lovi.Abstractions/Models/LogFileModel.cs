namespace CodeYesterday.Lovi.Models;

public class LogFileModel
{
    public required int Id { get; init; }

    public required string FilePath { get; init; }

    public long Size { get; set; }

    public long IncrementalImportStartPosition { get; set; }
}
