using CodeYesterday.Lovi.Importer;
using CodeYesterday.Lovi.Session;

namespace CodeYesterday.Lovi.Services;

public interface IImporterManager
{
    IDictionary<string, IImporter> Importers { get; }

    IDictionary<string, ImporterProfile> ImporterProfiles { get; }
}
