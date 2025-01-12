using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Radzen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FileInfo = System.IO.FileInfo;

namespace CodeYesterday.Lovi.Session;

internal class LogSession : IAsyncDisposable
{
    private const string DataDirectoryName = ".lovi";
    private const string SessionInfoFileName = ".session.lovi";

    public const string InMemorySessionDataStorageId = "InMemory";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static bool SessionInfoFileExists(string sessionDirectory)
    {
        return File.Exists(Path.Combine(sessionDirectory, SessionInfoFileName));
    }

    private LogItemModel? _selectedLogItem;
    private CancellationTokenSource? _importCts;
    private Task? _importTask;
    private string? _advancedFilterExpression;
    private bool _disableFilterChanged;

    public required IImporterManager ImporterManager { get; init; }

    public ISessionDataStorage? DataStorage { get; private set; }

    public ISessionConfigStorage? LogSessionConfigStorage { get; private set; }

    public string SessionDirectory { get; set; }

    public string DataDirectory => Path.Combine(SessionDirectory, DataDirectoryName);

    public string SessionInfoFilePath => Path.Combine(SessionDirectory, SessionInfoFileName);

    public SessionConfig SessionConfig { get; private set; }

    public LogLevelFilterModel LogLevelFilter { get; } = new();

    public IEnumerable<LogItemModel>? PagedItems { get; set; }

    public int FullLogEventCount => DataStorage?.FullLogEventCount ?? 0;

    public int FilteredLogEventCount { get; set; }

    public bool IsImporting => _importTask is not null;

    public LogItemModel? SelectedLogItem
    {
        get => _selectedLogItem;
        set
        {
            if (ReferenceEquals(value, _selectedLogItem)) return;
            var oldValue = _selectedLogItem;
            _selectedLogItem = value;
            SelectedLogItemModelChanged?.Invoke(this, new(oldValue, value));
        }
    }

    public string? AdvancedFilterExpression
    {
        get => _advancedFilterExpression;
        set
        {
            if (string.Equals(value, _advancedFilterExpression)) return;

            _advancedFilterExpression = value;
            Refresh();
        }
    }

    public event EventHandler? PropertiesChanged;

    public event EventHandler? Refreshing;

    public event EventHandler<ChangedEventArgs<LogItemModel>>? SelectedLogItemModelChanged;

    public LogSession(string sessionDirectory)
    {
        SessionDirectory = Path.GetFullPath(sessionDirectory);
        SessionConfig = new();
    }

    public async Task OpenSessionAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(SessionInfoFilePath))
        {
            throw new FileNotFoundException("Session info file not found", SessionInfoFilePath);
        }

        CheckDirectories();

        var sessionDataStorageId = await ReadSessionInfoFileAsync(cancellationToken).ConfigureAwait(false);

        if (!string.Equals(sessionDataStorageId, InMemorySessionDataStorageId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unsupported session data storage ID: {sessionDataStorageId}");
        }

        await InitInternalAsync(false, cancellationToken).ConfigureAwait(false);
    }

    public async Task CreateNewSessionAsync(string sessionDataStorageId, CancellationToken cancellationToken)
    {
        if (!string.Equals(sessionDataStorageId, InMemorySessionDataStorageId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Invalid session data storage ID", nameof(sessionDataStorageId));
        }

        CheckDirectories();

        await WriteSessionInfoFileAsync(sessionDataStorageId, cancellationToken).ConfigureAwait(false);

        await InitInternalAsync(true, cancellationToken).ConfigureAwait(false);
    }

    public Task SaveDataAsync(CancellationToken cancellationToken)
    {
        CheckInitialized();

        SessionConfig.WriteLogLevelFilterLayerSettings(LogLevelFilter);

        return LogSessionConfigStorage.WriteSessionConfigAsync(SessionConfig, SessionDirectory, DataDirectory, cancellationToken);
    }

    public async Task LoadDataAsync(LoadDataArgs args)
    {
        CheckInitialized();

        if (IsImporting) return;

        var logLevelFilter = LogLevelFilter.GetFilter();

        // Combine the filters.
        var filters = new List<string>();
        if (!string.IsNullOrEmpty(logLevelFilter))
        {
            Debug.Print($"LogLevelFilter: {logLevelFilter}");
            filters.Add(logLevelFilter);
        }

        if (!string.IsNullOrEmpty(AdvancedFilterExpression))
        {
            Debug.Print($"AdvancedFilter: {AdvancedFilterExpression}");
            filters.Add(AdvancedFilterExpression);
        }

        if (!string.IsNullOrEmpty(args.Filter))
        {
            Debug.Print($"DataGridFilter: {args.Filter}");
            filters.Add(args.Filter);
        }

        var filter = filters.Count switch
        {
            0 => string.Empty,
            1 => filters[0],
            _ => string.Join("and", filters.Select(f => $"({f})"))
        };
        Debug.Print($"CombinedFilter: {filter}");

        // Get the filtered items:
        var (data, count) = await DataStorage.GetDataAsync(args.Skip, args.Top,
            args.OrderBy, filter, CancellationToken.None).ConfigureAwait(true);

        // Update the data:
        FilteredLogEventCount = count;
        PagedItems = data.ToList();
    }

    public async Task ImportDataAsync(IProgressIndicator progressIndicator, CancellationToken cancellationToken)
    {
        CheckInitialized();

        if (IsImporting) throw new InvalidOperationException("Import already running");

        if (!Directory.Exists(SessionDirectory))
        {
            throw new InvalidOperationException("The session directory does not exist");
        }

        var progressModel = new ProgressModel()
        {
            Id = "Import",
            Action = "Importing log files",
            MainProgress = new()
            {
                Max = 100,
                Value = 100,
                IsIndeterminate = true
            },
            CancelCallback = () =>
            {
                CancelImport();
                return Task.CompletedTask;
            },
            CanCancel = true
        };
        progressIndicator.SetProgressModel(progressModel);

        _importCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            await ImportDataAsync(progressModel, _importCts.Token).ConfigureAwait(false);
        }
        finally
        {
            _importCts?.Dispose();
            _importCts = null;
            progressIndicator.ClearProgressModel("Import");
        }
    }

    public void CancelImport()
    {
        _importCts?.Cancel();
    }


    public void Refresh()
    {
        Refreshing?.Invoke(this, EventArgs.Empty);
    }

    public async ValueTask DisposeAsync()
    {
        CheckInitialized();

        await UnloadDataAsync(CancellationToken.None);

        SessionConfig.Sources.Clear();

        await CastAndDispose(DataStorage);

        return;

        static async ValueTask CastAndDispose(object resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
            {
                await resourceAsyncDisposable.DisposeAsync();
            }
            else if (resource is IDisposable resourceDisposable)
            {
                resourceDisposable.Dispose();
            }
        }
    }

    internal async Task UnloadDataAsync(CancellationToken cancellationToken)
    {
        CheckInitialized();

        if (_importCts != null)
        {
            await _importCts.CancelAsync();
        }

        if (_importTask != null)
        {
            try
            {
                await _importTask.ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        _importCts?.Dispose();

        _importTask = null;
        _importCts = null;

        await DataStorage.ClearDataAsync(cancellationToken);
    }

    private async Task InitInternalAsync(bool isNew, CancellationToken cancellationToken)
    {
        DataStorage = new InMemorySessionDataStorage();

        // ReSharper disable once SuspiciousTypeConversion.Global
        LogSessionConfigStorage = DataStorage as ISessionConfigStorage ?? new FileSessionConfigStorage();

        if (isNew)
        {
            await DataStorage.CreateAsync(SessionDirectory, DataDirectory, cancellationToken).ConfigureAwait(false);

            SessionConfig = new();

            await SaveDataAsync(CancellationToken.None).ConfigureAwait(true);
        }
        else
        {
            await DataStorage.OpenAsync(SessionDirectory, DataDirectory, cancellationToken).ConfigureAwait(false);

            SessionConfig = await LogSessionConfigStorage.ReadSessionConfigAsync(SessionDirectory, DataDirectory, cancellationToken).ConfigureAwait(false);
        }

        DataStorage.PropertiesChanged += (_, args) => PropertiesChanged?.Invoke(this, args);
        LogLevelFilter.FilterChanged += OnFilterChanged;
    }

    [MemberNotNull(nameof(DataStorage))]
    [MemberNotNull(nameof(LogSessionConfigStorage))]
    public void CheckInitialized()
    {
        if (DataStorage is null || LogSessionConfigStorage is null) throw new InvalidOperationException("Session is not initialized");
    }

    private async Task WriteSessionInfoFileAsync(string sessionDataStorageId, CancellationToken cancellationToken)
    {
        await using var fileStream = File.Create(SessionInfoFilePath);

        await JsonSerializer.SerializeAsync(fileStream,
            new SessionInfoFile
            {
                SessionDataStorageId = sessionDataStorageId
            },
            JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string?> ReadSessionInfoFileAsync(CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(SessionInfoFilePath);

        var sessionInfo = await JsonSerializer.DeserializeAsync<SessionInfoFile>(fileStream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (sessionInfo is null) throw new InvalidOperationException("Could not read session info file.");

        return sessionInfo.SessionDataStorageId;
    }

    private void CheckDirectories()
    {
        if (!Directory.Exists(SessionDirectory))
        {
            throw new InvalidOperationException("The session directory does not exist.");
        }

        if (!Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
            var attr = File.GetAttributes(DataDirectory);
            attr |= FileAttributes.Hidden;
            File.SetAttributes(DataDirectory, attr);
        }
    }

    private async Task ImportDataAsync(ProgressModel progressModel, CancellationToken cancellationToken)
    {
        CheckInitialized();

        foreach (var importSource in SessionConfig.Sources)
        {
            if (importSource.ImportAllFiles)
            {
                var dir = Path.GetDirectoryName(importSource.Filter);
                if (string.IsNullOrEmpty(dir))
                {
                    dir = SessionDirectory;
                }
                else if (!Path.IsPathRooted(dir))
                {
                    dir = Path.Combine(SessionDirectory, dir);
                }

                importSource.SelectedFiles = Directory.GetFiles(dir, Path.GetFileName(importSource.Filter));
            }
        }

        var files = SessionConfig.Sources
            .SelectMany(s => s.SelectedFiles.Select(f => new { source = s, file = new FileInfo(f) }))
            .OrderBy(i => i.file.LastWriteTimeUtc)
            .ToArray();

        if (!files.Any()) return;

        progressModel.MainProgress = new()
        {
            Max = files.Sum(i => i.file.Length),
            Value = 0
        };

        foreach (var file in files)
        {
            progressModel.Item = file.file.Name;
            progressModel.SecondaryProgress = new() { Max = 1, Value = 0 };

            if (!ImporterManager.ImporterProfiles.TryGetValue(file.source.ImporterProfileId,
                    out var importerProfile))
            {
                throw new InvalidOperationException(
                    $"The importer profile with ID {file.source.ImporterProfileId} does not exist");
            }

            if (!ImporterManager.Importers.TryGetValue(importerProfile.ImporterId,
                    out var importer))
            {
                throw new InvalidOperationException(
                    $"The importer with ID {importerProfile.ImporterId} does not exist");
            }

            // TODO: Support incremental load.
            await DataStorage.ImportDataAsync(importer, file.file.FullName, 0, SessionConfig.StartTime, SessionConfig.EndTime,
                    progressModel, cancellationToken)
                .ConfigureAwait(false);
        }

        progressModel.Item = string.Empty;
        progressModel.Action = "Build filter tree";
        progressModel.CanCancel = false;
        progressModel.MainProgress = new() { IsIndeterminate = true };
        progressModel.SecondaryProgress = null;

        const string? logLevelFilterId = null;

        // ReSharper disable once RedundantArgumentDefaultValue
        SessionConfig.ApplyLogLevelFilterPropertySettings(LogLevelFilter, logLevelFilterId);

        _disableFilterChanged = true;
        try
        {
            await DataStorage.UpdateLogLevelFilterAsync(LogLevelFilter, cancellationToken).ConfigureAwait(false);

            // ReSharper disable once RedundantArgumentDefaultValue
            SessionConfig.ApplyLogLevelFilterLayerSettings(LogLevelFilter, logLevelFilterId);
        }
        finally
        {
            _disableFilterChanged = false;
            OnFilterChanged(null, EventArgs.Empty);
        }
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        if (_disableFilterChanged) return;

        Refresh();

        _ = SaveDataAsync(CancellationToken.None);
    }
}
