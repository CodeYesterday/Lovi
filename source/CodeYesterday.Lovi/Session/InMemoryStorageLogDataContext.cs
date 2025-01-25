namespace CodeYesterday.Lovi.Session;

internal class InMemoryStorageLogDataContext
{
    public int FilteredCount { get; set; } = -1;

    public string LastFilter { get; set; } = string.Empty;

    public string OrderBy { get; set; } = string.Empty;
}