using Serilog.Events;
using System.Text;

namespace CodeYesterday.Lovi.Models;

[PublicAPI]
public class LogLevelFilterLayerModel
{
    private readonly bool?[] _showLevels = new bool?[6];
    private bool? _showLayer;

    public LogLevelFilterModel? Filter { get; set; }

    public string? Property { get; }

    public string? Value { get; set; }

    public int LayerIndex { get; }

    public bool IsShowLayerTriState => LayerIndex > 1;

    public bool IsLevelTriState => LayerIndex > 0;

    public bool? ShowLayer
    {
        get => _showLayer;
        set
        {
            if (value == _showLayer) return;
            _showLayer = value;
            Filter?.OnFilterChanged();
        }
    }

    public IReadOnlyList<bool?> ShowLevels => _showLevels;

    public bool? this[LogEventLevel level]
    {
        get => _showLevels[(int)level];
        set
        {
            if (value == _showLevels[(int)level]) return;
            _showLevels[(int)level] = value;
            Filter?.OnFilterChanged();
        }
    }

    public bool? ShowFatal
    {
        get => this[LogEventLevel.Fatal];
        set => this[LogEventLevel.Fatal] = value;
    }

    public bool? ShowError
    {
        get => this[LogEventLevel.Error];
        set => this[LogEventLevel.Error] = value;
    }

    public bool? ShowWarning
    {
        get => this[LogEventLevel.Warning];
        set => this[LogEventLevel.Warning] = value;
    }

    public bool? ShowInformation
    {
        get => this[LogEventLevel.Information];
        set => this[LogEventLevel.Information] = value;
    }

    public bool? ShowDebug
    {
        get => this[LogEventLevel.Debug];
        set => this[LogEventLevel.Debug] = value;
    }

    public bool? ShowVerbose
    {
        get => this[LogEventLevel.Verbose];
        set => this[LogEventLevel.Verbose] = value;
    }

    public bool IsExpanded { get; set; } = true;

    public List<LogLevelFilterLayerModel> SubLayers { get; set; } = new();

    internal bool IsDefault
    {
        get
        {
            if (LayerIndex > 0 && !(IsShowLayerTriState && ShowLayer is null || !IsShowLayerTriState && ShowLayer is true)) return false;

            if (IsLevelTriState)
            {
                if (!(ShowFatal is null && ShowError is null && ShowWarning is null &&
                      ShowInformation is null && ShowDebug is null && ShowVerbose is null)) return false;
            }
            else
            {
                if (!(ShowFatal is true && ShowError is true && ShowWarning is true &&
                      ShowInformation is true && ShowDebug is true && ShowVerbose is true)) return false;
            }

            if (!SubLayers.All(l => l.IsDefault)) return false;

            return true;
        }
    }

    public LogLevelFilterLayerModel(LogLevelFilterModel filter, string? property, string? value, int layerIndex)
    {
        Filter = filter;
        Property = property;
        Value = value;
        LayerIndex = layerIndex;

        if (!IsShowLayerTriState)
        {
            ShowLayer = true;
        }

        if (!IsLevelTriState)
        {
            ShowFatal = true;
            ShowError = true;
            ShowWarning = true;
            ShowInformation = true;
            ShowDebug = true;
            ShowVerbose = true;
        }
    }

    public void GetFilter(StringBuilder filter, Stack<KeyValuePair<string, string?>> propertyValues, bool visible,
        bool[] showLayer)
    {
        if (Property is not null)
        {
            propertyValues.Push(new(Property, Value));
        }

        try
        {
            if (ShowLayer.HasValue) visible = ShowLayer.Value;
            foreach (var level in Enum.GetValues<LogEventLevel>())
            {
                if (this[level].HasValue) showLayer[(int)level] = this[level]!.Value;
            }

            if (SubLayers.Any() && SubLayers.Any(l => !l.IsDefault))
            {
                foreach (var subLayer in SubLayers)
                {
                    // MUST pass a copy of the showLayer array in!
                    subLayer.GetFilter(filter, propertyValues, visible, showLayer.ToArray());
                }

                return;
            }

            // If nothing is visible, don't add it to the filter.
            if (!visible || showLayer.All(show => !show)) return;

            if (filter.Length > 0)
            {
                filter.Append("or");
            }

            filter.Append("(");

            filter.Append(string.Join("and",
                propertyValues
                    .Reverse()
                    .Select(pv => $"(String(it[\"s:{pv.Key}\"])=={(pv.Value is null ? "null" : $"\"{pv.Value.Replace("\"", "\\\"")}\"")})")));

            if (showLayer.Any(show => !show))
            {
                filter.Append("and");
                LogLevelFilterModel.AppendLogEventLevelFilter(filter, showLayer);
            }

            filter.Append(")");
        }
        finally
        {
            if (Property is not null)
            {
                propertyValues.Pop();
            }
        }
    }

    public void ExpandCollapseAll(bool expand)
    {
        IsExpanded = expand;
        foreach (var subLayer in SubLayers)
        {
            subLayer.ExpandCollapseAll(expand);
        }
    }
}
