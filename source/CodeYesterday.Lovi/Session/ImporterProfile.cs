namespace CodeYesterday.Lovi.Session;

public class ImporterProfile
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required string ImporterId { get; set; }

    public required string DefaultSourceFilter { get; set; }

    public bool IsBaseProfile { get; set; }
}
