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
    private int _filteredCount = -1;
    private string _lastFilter = string.Empty;

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

        var propertiesChanged = false;
        await foreach (var evt in importer.ImportLogsAsync(filePath,
                           progress =>
                           {
                               var diff = progress.Value - progressModel.SecondaryProgress?.Value ?? 0;
                               progressModel.SecondaryProgress = progress;
                               progressModel.MainProgress = progressModel.MainProgress with
                               {
                                   Value = progressModel.MainProgress.Value + diff
                               };
                           },
                           cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ((startTime is null || evt.Timestamp >= startTime)
                || (endTime is null || evt.Timestamp <= endTime))
            {
                propertiesChanged |= AddInternal(evt, file.Id);
            }
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

        OnPropertiesChanged();

        return Task.CompletedTask;
    }

    public async Task<(IQueryable<LogItemModel>, int)> GetDataAsync(int? skip, int? take,
        string orderBy, string filter,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IQueryable<LogItemModel> query = string.IsNullOrEmpty(orderBy) ?
            _data.OrderBy(item => item.LogEvent.Timestamp).AsQueryable() :
            _data.AsQueryable().OrderBy(orderBy);

        int count;
        if (string.IsNullOrEmpty(filter))
        {
            count = _data.Count;
        }
        else
        {
            query = query.Where(filter);
            if (_filteredCount < 0 || !string.Equals(filter, _lastFilter, StringComparison.Ordinal))
            {
                count = await Task.FromResult(query.Count());
            }
            else
            {
                count = _filteredCount;
            }
        }
        _lastFilter = filter;
        _filteredCount = count;

        if (skip is not null)
        {
            query = query.Skip(skip.Value);
        }
        if (take is not null)
        {
            query = query.Take(take.Value);
        }

        return (query, _filteredCount);
    }

    private bool AddInternal(LogEvent evt, int fileId)
    {
        _filteredCount = -1;

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

    private void OnPropertiesChanged()
    {
        PropertiesChanged?.Invoke(this, EventArgs.Empty);
    }
}
