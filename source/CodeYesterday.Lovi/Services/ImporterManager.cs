using CodeYesterday.Lovi.Importer;
using CodeYesterday.Lovi.Importer.Clef;
using CodeYesterday.Lovi.Session;

namespace CodeYesterday.Lovi.Services;

internal class ImporterManager : IImporterManager
{
    public IDictionary<string, IImporter> Importers { get; } = new Dictionary<string, IImporter>();

    public IDictionary<string, ImporterProfile> ImporterProfiles { get; } = new Dictionary<string, ImporterProfile>();

    public ImporterManager()
    {
        Importers.Add("CLEF", new ClefImporter());

        ImporterProfiles.Add("CLEF", new()
        {
            Id = "CLEF",
            Name = "CLEF",
            IsBaseProfile = true,
            ImporterId = "CLEF",
            DefaultSourceFilter = "*.clef"
        });
    }
}
