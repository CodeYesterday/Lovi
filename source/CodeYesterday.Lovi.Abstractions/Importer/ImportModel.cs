namespace CodeYesterday.Lovi.Importer;

[PublicAPI]
public record ImportModel
{
    public required DateTime Timestamp { get; init; }

    public string? Id { get; init; }
}
