using CodeYesterday.Lovi.Session;

namespace CodeYesterday.Lovi.Services;

/// <summary>
/// Interface for the session service.
/// </summary>
[PublicAPI]
public interface ISessionService
{
    /// <summary>
    /// Gets the list of active sessions.
    /// </summary>
    IReadOnlyList<LogSession> Sessions { get; }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="sessionDirectory">The session directory.</param>
    /// <param name="importerManager">The importer manager to resolve the importer.</param>
    /// <returns>Returns the created session.</returns>
    LogSession CreateSession(string sessionDirectory, IImporterManager importerManager);

    /// <summary>
    /// Closes the session.
    /// </summary>
    /// <param name="sessionId">The ID of the session.</param>
    ValueTask CloseSessionAsync(int sessionId);

    /// <summary>
    /// Gets a session by its ID.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <returns>Returns the session or <see langword="null"/> if not session with the given ID exists.</returns>
    LogSession? GetSession(int sessionId);
}