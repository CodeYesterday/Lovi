namespace CodeYesterday.Lovi.Session;

[PublicAPI]
public class ImportSource
{
    public required string Name { get; init; }

    public required string ImporterProfileId { get; init; }

    public required string Filter { get; set; }

    public bool ImportAllFiles { get; set; }

    public IList<string> SelectedFiles { get; set; } = [];
}