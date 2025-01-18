using CodeYesterday.Lovi.Importer;
using CodeYesterday.Lovi.Models;

namespace CodeYesterday.Lovi.Session;

/// <summary>
/// Interface for log item data storage implementations.
/// </summary>
[PublicAPI]
public interface ISessionDataStorage
{
    /// <summary>
    /// Gets the full number of log items.
    /// </summary>
    public int FullLogEventCount { get; }

    /// <summary>
    /// Gets the dictionary wih all files. The key is the <see cref="LogFileModel.Id"/>.
    /// </summary>
    public IDictionary<int, LogFileModel> LogFiles { get; }

    /// <summary>
    /// Gets all log item properties.
    /// </summary>
    ICollection<LogPropertyModel> Properties { get; }

    /// <summary>
    /// Occurs if the properties have changed.
    /// </summary>
    event EventHandler? PropertiesChanged;

    /// <summary>
    /// Creates and initializes a new session data storage.
    /// </summary>
    /// <param name="sessionDirectory">The session directory.</param>
    /// <param name="dataDirectory">The data directory.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task CreateAsync(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken);

    /// <summary>
    /// Open and initializes an existing session data storage.
    /// </summary>
    /// <param name="sessionDirectory">The session directory.</param>
    /// <param name="dataDirectory">The data directory.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task OpenAsync(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken);

    /// <summary>
    /// Returns information for the log data.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns the information data.</returns>
    Task<LogDataStats> GetStatsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Imports the log events from a file.
    /// </summary>
    /// <param name="importer">The importer to import the file.</param>
    /// <param name="filePath">The full path of the file to import.</param>
    /// <param name="startFilePosition">The start byte position in the file. MUST be 0 if the importer does not support incremental import.</param>
    /// <param name="startTime">The minimum time of a log item to import.</param>
    /// <param name="endTime">The maximum time of a log item to import.</param>
    /// <param name="progressModel">The <see cref="ProgressModel"/> for progress updates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the import.</param>
    Task ImportDataAsync(IImporter importer, string filePath, long startFilePosition, DateTimeOffset? startTime, DateTimeOffset? endTime,
        ProgressModel progressModel, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the filter tree.
    /// </summary>
    /// <param name="filter">The <see cref="LogLevelFilterModel"/> to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the update.</param>
    /// <returns></returns>
    Task UpdateLogLevelFilterAsync(LogLevelFilterModel filter, CancellationToken cancellationToken);

    /// <summary>
    /// Removes all log items of a specific file from the storage.
    /// </summary>
    /// <param name="fileId">The ID of the file.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    Task UnloadFileAsync(int fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Clears all data from the storage.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns></returns>
    Task ClearDataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// returns the log file with the matching file path.
    /// </summary>
    /// <param name="filePath">The file path of the file.</param>
    /// <returns>Returns the <see cref="LogFileModel"/> or <see langword="null"/> if it was not found.</returns>
    LogFileModel? GetLogFileByFilePath(string filePath)
    {
        // TODO: switch ignore case based on platform.
        return LogFiles.Values.FirstOrDefault(f =>
            string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns a filtered subset of the log items and the count of filtered items.
    /// </summary>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take.</param>
    /// <param name="orderBy">Ordering instructions.</param>
    /// <param name="filter">The filter expression.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>Returns the subset of log items and the total count of items with the given filter applied.</returns>
    Task<(IQueryable<LogItemModel>, int)> GetDataAsync(int? skip, int? take, string orderBy, string filter,
        CancellationToken cancellationToken);
}