using CodeYesterday.Lovi.Session;

namespace CodeYesterday.Lovi.Services;

public class SessionService : ISessionService
{
    private readonly List<LogSession> _sessions = new();

    public IReadOnlyList<LogSession> Sessions => _sessions.AsReadOnly();

    public LogSession CreateSession(string sessionDirectory, IImporterManager importerManager)
    {
        var session = new LogSession(sessionDirectory)
        {
            SessionId = Sessions.Count == 0 ? 1 : Sessions.Max(s => s.SessionId) + 1,
            ImporterManager = importerManager
        };

        _sessions.Add(session);

        return session;
    }

    public ValueTask CloseSessionAsync(int sessionId)
    {
        var session = GetSession(sessionId);

        if (session is not null)
        {
            _sessions.Remove(session);
            return session.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }

    public LogSession? GetSession(int sessionId)
    {
        return Sessions.FirstOrDefault(s => s.SessionId == sessionId);
    }
}