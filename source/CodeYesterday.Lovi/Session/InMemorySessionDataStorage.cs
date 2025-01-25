using CodeYesterday.Lovi.Importer;
using CodeYesterday.Lovi.Models;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;

namespace CodeYesterday.Lovi.Session;

internal class InMemorySessionDataStorage : ISessionDataStorage
{
    private readonly List<LogItemModel> _data = new();
    private readonly ConcurrentDictionary<string, LogPropertyModel> _properties = new();
    private readonly List<InMemoryStorageLogDataContext> _logDataContexts = new();

    public int FullLogEventCount => _data.Count;

    public IDictionary<int, LogFileModel> LogFiles { get; } = new Dictionary<int, LogFileModel>();

    public ICollection<LogPropertyModel> Properties => _properties.Values;

    public event EventHandler? PropertiesChanged;

    public Task CreateAsync(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken)
    {
        // nothing to do
        return Task.CompletedTask;
    }

    public Task OpenAsync(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken)
    {
        // nothing to do
        return Task.CompletedTask;
    }

    public Task<LogDataStats> GetStatsAsync(CancellationToken cancellationToken)
    {
        if (_data.Count == 0)
        {
            return Task.FromResult(LogDataStats.Empty);
        }

        return Task.FromResult(_data.Aggregate(
            new LogDataStats
            {
                LogItemCount = _data.Count,
                FirstItemTimestamp = DateTimeOffset.MaxValue,
                LastItemTimestamp = DateTimeOffset.MinValue
            },
            (a, x) =>
                a with
                {
                    FirstItemTimestamp = x.LogEvent.Timestamp < a.FirstItemTimestamp ? x.LogEvent.Timestamp : a.FirstItemTimestamp,
                    LastItemTimestamp = x.LogEvent.Timestamp > a.LastItemTimestamp ? x.LogEvent.Timestamp : a.LastItemTimestamp,
                }));
    }

    public async Task ImportDataAsync(IImporter importer, string filePath,
        long startFilePosition, DateTimeOffset? startTime, DateTimeOffset? endTime,
        ProgressModel progressModel, CancellationToken cancellationToken)
    {
        // TODO: implement incremental load.

        var file = ((ISessionDataStorage)this).GetLogFileByFilePath(filePath);
        if (file is null)
        {
            var fileId = LogFiles.Any() ? LogFiles.Values.Max(f => f.Id) + 1 : 0;

            file = new()
            {
                FilePath = filePath,
                Id = fileId
            };
            LogFiles.Add(file.Id, file);
        }

        file.Size = new FileInfo(filePath).Length;

        progressModel.SecondaryProgress = new()
        {
            Max = file.Size,
            Value = 0
        };

        var propertiesChanged = false;
        try
        {
            double initialMainProgressValue = progressModel.MainProgress.Value;

            await foreach (var (evt, filePosition) in importer.ImportLogsAsync(filePath,
                               cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((startTime is null || evt.Timestamp >= startTime)
                    || (endTime is null || evt.Timestamp <= endTime))
                {
                    propertiesChanged |= AddInternal(evt, file.Id);
                }

                progressModel.SecondaryProgress = new()
                {
                    Max = file.Size,
                    Value = filePosition
                };
                progressModel.MainProgress = progressModel.MainProgress with
                {
                    Value = initialMainProgressValue + filePosition
                };

                file.IncrementalImportStartPosition = filePosition;
            }
        }
        catch (OperationCanceledException)
        {
            file.ImportCancelled = true;
            throw;
        }

        if (propertiesChanged)
        {
            OnPropertiesChanged();
        }
    }

    public Task UpdateLogLevelFilterAsync(LogLevelFilterModel filter, CancellationToken cancellationToken)
    {
        void FillLayer(LogLevelFilterLayerModel layer, IList<string> layerProperties, IEnumerable<LogItemModel> items)
        {
            if (layer.LayerIndex >= layerProperties.Count) return;

            var property = layerProperties[layer.LayerIndex];

            foreach (var grouping in items.GroupBy(i => i.GetPropertyScalarStringValue(property)))
            {
                var subLayer = new LogLevelFilterLayerModel(filter, property, grouping.Key, layer.LayerIndex + 1);
                layer.SubLayers.Add(subLayer);

                FillLayer(subLayer, layerProperties, grouping);
            }
        }

        filter.RootLayer.SubLayers.Clear();

        // Filter layer properties for only existing properties with a string value.
        var existingLayerProperties = new List<string>();

        foreach (var layerProperty in filter.LayerProperties)
        {
            var property = Properties.FirstOrDefault(p => string.Equals(layerProperty, p.Name, StringComparison.Ordinal));

            if (property is null || !property.PropertyTypes.Contains(PropertyType.String)) continue;

            existingLayerProperties.Add(layerProperty);
        }

        FillLayer(filter.RootLayer, existingLayerProperties, _data);

        return Task.CompletedTask;
    }

    public Task UnloadFileAsync(int fileId, CancellationToken cancellationToken)
    {
        // TODO: Log items of one file usually are right next to each other in the list: Search start/end of each block and use RemoveRange()
        for (int n = _data.Count - 1; n >= 0; --n)
        {
            if (_data[n].FileId == fileId)
            {
                _data.RemoveAt(n);
            }
        }

        LogFiles.Remove(fileId);

        return Task.CompletedTask;
    }

    public Task AddLogEventAsync(LogEvent evt, int fileId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var propertiesChanged = AddInternal(evt, fileId);

        if (propertiesChanged)
        {
            OnPropertiesChanged();
        }

        return Task.CompletedTask;
    }

    public Task ClearDataAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _data.Clear();
        _properties.Clear();
        LogFiles.Clear();

        OnPropertiesChanged();

        return Task.CompletedTask;
    }

    public object OpenLogDataContext()
    {
        var inMemoryContext = new InMemoryStorageLogDataContext();
        lock (_logDataContexts)
        {
            _logDataContexts.Add(inMemoryContext);
        }
        return inMemoryContext;
    }

    public void CloseLogDataContext(object context)
    {
        var inMemoryContext = context as InMemoryStorageLogDataContext ?? throw new ArgumentException("Invalid log data context", nameof(context));

        lock (_logDataContexts)
        {
            _logDataContexts.Remove(inMemoryContext);
        }
    }

    public async Task<(IQueryable<LogItemModel>, int)> GetDataAsync(int? skip, int? take,
        string orderBy, string filter, object context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var inMemoryContext = context as InMemoryStorageLogDataContext ?? throw new ArgumentException("Invalid log data context", nameof(context));

        inMemoryContext.OrderBy = orderBy;

        var query = GetDataAsOrderedQueryable(inMemoryContext, false);

        int count;
        if (string.IsNullOrEmpty(filter))
        {
            count = _data.Count;
        }
        else
        {
            query = query.Where(filter);
            if (inMemoryContext.FilteredCount < 0 || !string.Equals(filter, inMemoryContext.LastFilter, StringComparison.Ordinal))
            {
                count = await Task.FromResult(query.Count());
            }
            else
            {
                count = inMemoryContext.FilteredCount;
            }
        }
        inMemoryContext.LastFilter = filter;
        inMemoryContext.FilteredCount = count;

        if (skip is not null)
        {
            query = query.Skip(skip.Value);
        }
        if (take is not null)
        {
            query = query.Take(take.Value);
        }

        return (query, inMemoryContext.FilteredCount);
    }

    public async Task<(LogItemModel item, int index)?> GetLogItemAndIndexAsync(LogItemModel item, bool exact, object context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var inMemoryContext = context as InMemoryStorageLogDataContext ?? throw new ArgumentException("Invalid log data context", nameof(context));

        if (inMemoryContext.FilteredCount == 0) return null;

        var data = GetDataAsOrderedQueryable(inMemoryContext, true);

        var tuple = GetItemAndIndexOf(data, x => x.Id == item.Id);

        if (tuple is null || exact) return tuple;

        return await GetLogItemAndIndexAsync(item.LogEvent.Timestamp, false, context, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public Task<(LogItemModel item, int index)?> GetLogItemAndIndexAsync(DateTimeOffset timestamp, bool exact, object context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var inMemoryContext = context as InMemoryStorageLogDataContext ?? throw new ArgumentException("Invalid log data context", nameof(context));

        if (inMemoryContext.FilteredCount == 0) return Task.FromResult<(LogItemModel item, int index)?>(null);

        var data = GetDataAsOrderedQueryable(inMemoryContext, true);

        if (exact)
        {
            return Task.FromResult(GetItemAndIndexOf(data, x => x.LogEvent.Timestamp == timestamp));
        }

        var e = data as IEnumerable<LogItemModel>;
        return Task.FromResult<(LogItemModel item, int index)?>(e
            .Select((item, index) => (item, index))
            .MinBy(x => Math.Abs((x.item.LogEvent.Timestamp - timestamp).Ticks)));
    }

    public Task<LogItemModel?> GetLogItemByIndexAsync(int index, object context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var inMemoryContext = context as InMemoryStorageLogDataContext ?? throw new ArgumentException("Invalid log data context", nameof(context));

        if (inMemoryContext.FilteredCount == 0) return Task.FromResult<LogItemModel?>(null);

        var data = GetDataAsOrderedQueryable(inMemoryContext, true);

        return Task.FromResult(data.Skip(index).FirstOrDefault());
    }

    private (LogItemModel item, int index)? GetItemAndIndexOf(IEnumerable<LogItemModel> items, Predicate<LogItemModel> predicate)
    {
        return items
            .Select((item, index) => (item, index))
            .FirstOrDefault(x => predicate(x.item));
    }

    private IQueryable<LogItemModel> GetDataAsOrderedQueryable(InMemoryStorageLogDataContext context, bool filtered)
    {
        var data = string.IsNullOrEmpty(context.OrderBy) ?
            _data.OrderBy(item => item.LogEvent.Timestamp).AsQueryable() :
            _data.AsQueryable().OrderBy(context.OrderBy);

        if (!filtered || string.IsNullOrEmpty(context.LastFilter)) return data;

        return data.Where(context.LastFilter);
    }

    private bool AddInternal(LogEvent evt, int fileId)
    {
        ResetAllContextFilteredCount();

        var logItemModel = new LogItemModel()
        {
            Id = _data.Count,
            FileId = fileId,
            LogEvent = evt
        };
        _data.Add(logItemModel);

        var changed = false;
        foreach (var property in evt.Properties)
        {
            var nextId = _properties.Count;
            if (!_properties.TryGetValue(property.Key, out var p))
            {
                p = new LogPropertyModel() { Id = nextId, Name = property.Key };
                _properties.TryAdd(property.Key, p);
                changed = true;
            }

            var propertyType = logItemModel.GetPropertyType(property.Key);
            if (!p.PropertyTypes.Contains(propertyType))
            {
                p.PropertyTypes.Add(propertyType);
                changed = true;
            }
        }

        return changed;
    }

    private void ResetAllContextFilteredCount()
    {
        lock (_logDataContexts)
        {
            foreach (var inMemoryContext in _logDataContexts)
            {
                inMemoryContext.FilteredCount = -1;
            }
        }
    }

    private void OnPropertiesChanged()
    {
        PropertiesChanged?.Invoke(this, EventArgs.Empty);
    }
}
