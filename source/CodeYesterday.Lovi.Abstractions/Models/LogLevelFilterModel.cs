using Serilog.Events;
using System.Text;

namespace CodeYesterday.Lovi.Models;

[PublicAPI]
public class LogLevelFilterModel
{
    public IList<string> LayerProperties { get; set; } = [];

    public IEnumerable<string> ActualLayerProperties
    {
        get
        {
            var layer = RootLayer;
            while (layer is not null)
            {
                if (layer.Property is not null)
                {
                    yield return layer.Property;
                }

                layer = layer.SubLayers.FirstOrDefault();
            }
        }
    }

    public LogLevelFilterLayerModel RootLayer { get; }

    public event EventHandler? FilterChanged;

    public LogLevelFilterModel()
    {
        RootLayer = new(this, null, null, 0);
    }

    public string GetFilter()
    {
        if (RootLayer.IsDefault) return string.Empty;

        var filter = new StringBuilder();

        if (RootLayer.SubLayers.All(l => l.IsDefault))
        {
            // Shortcut: global log event level only filter
            AppendLogEventLevelFilter(filter, RootLayer.ShowLevels.Select(l => l != false).ToArray());
        }
        else
        {
            var propertyValues = new Stack<KeyValuePair<string, string?>>();
            foreach (var subLayer in RootLayer.SubLayers)
            {
                subLayer.GetFilter(filter, propertyValues, true,
                    RootLayer.ShowLevels.Select(show => show ?? true).ToArray());
            }
        }

        return filter.ToString();
    }

    internal void OnFilterChanged()
    {
        FilterChanged?.Invoke(this, EventArgs.Empty);
    }

    internal static void AppendLogEventLevelFilter(StringBuilder filter, bool[] showLayer)
    {
        filter.Append($"({nameof(LogItemModel.LogEvent)}.{nameof(LogEvent.Level)} in ({string.Join(",",
            Enumerable.Range(0, showLayer.Length)
                .Where(l => showLayer[l])
                .Select(l => l))}))");
        //filter.Append(
        //    $"({string.Join("or",
        //        Enumerable.Range(0, showLayer.Length)
        //            .Where(l => showLayer[l])
        //            .Select(l => $"(np({nameof(LogItemModel.LogEvent)}.{nameof(LogEvent.Level)})={l})"))})");
    }
}