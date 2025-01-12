namespace CodeYesterday.Lovi.Services;

public interface IMruService
{
    IEnumerable<string> GetMruSessionDirectories();

    void SetMruSessionDirectory(string sessionDirectory);
}
