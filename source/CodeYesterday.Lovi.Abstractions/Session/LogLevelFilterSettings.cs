namespace CodeYesterday.Lovi.Session;

[PublicAPI]
public class LogLevelFilterSettings
{
    public IList<string> Properties { get; set; } = [];

    public IDictionary<string, string> LayersSettings { get; set; } = new Dictionary<string, string>();
}