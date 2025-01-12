using CodeYesterday.Lovi.Models;
using Serilog.Events;

namespace CodeYesterday.Lovi.Session;

[PublicAPI]
public class SessionConfig
{
    public List<ImportSource> Sources { get; init; } = new();

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public IDictionary<string, LogLevelFilterSettings> LogLevelFilterSettings { get; set; } = new Dictionary<string, LogLevelFilterSettings>();

    public void WriteLogLevelFilterLayerSettings(LogLevelFilterModel filter, string? settingsId = null)
    {
        if (!LogLevelFilterSettings.TryGetValue(settingsId ?? string.Empty, out var settings))
        {
            settings = new LogLevelFilterSettings
            {
                Properties = new List<string>(filter.LayerProperties)
            };
            LogLevelFilterSettings.Add(settingsId ?? string.Empty, settings);
        }
        else
        {
            settings.LayersSettings.Clear();
        }

        Stack<string> propertyStack = new();

        WriteLogLevelFilterLayerSettings(filter.RootLayer, propertyStack, settings);
    }

    public IList<string> GetLogLevelFilterPropertySettings(string? settingsId = null)
    {
        if (!LogLevelFilterSettings.TryGetValue(settingsId ?? string.Empty, out var settings))
        {
            return [];
        }

        return new List<string>(settings.Properties);
    }

    public void WriteLogLevelFilterPropertySettings(IList<string> properties, string? settingsId = null)
    {
        if (!LogLevelFilterSettings.TryGetValue(settingsId ?? string.Empty, out var settings))
        {
            settings = new LogLevelFilterSettings
            {
                Properties = new List<string>(properties)
            };
            LogLevelFilterSettings.Add(settingsId ?? string.Empty, settings);
        }
        else
        {
            settings.Properties = new List<string>(properties);
            settings.LayersSettings.Clear();
        }
    }

    public void ApplyLogLevelFilterPropertySettings(LogLevelFilterModel filter, string? settingsId = null)
    {
        if (!LogLevelFilterSettings.TryGetValue(settingsId ?? string.Empty, out var settings))
        {
            filter.LayerProperties = [];
            return;
        }

        filter.LayerProperties = new List<string>(settings.Properties);
    }

    public void ApplyLogLevelFilterLayerSettings(LogLevelFilterModel filter, string? settingsId = null)
    {
        if (!LogLevelFilterSettings.TryGetValue(settingsId ?? string.Empty, out var settings)) return;

        Stack<string> propertyStack = new();

        ApplyLogLevelFilterProperties(filter.RootLayer, propertyStack, settings);
    }

    private void WriteLogLevelFilterLayerSettings(LogLevelFilterLayerModel layer, Stack<string> propertyStack,
        LogLevelFilterSettings settings)
    {
        if (!string.IsNullOrEmpty(layer.Property))
        {
            propertyStack.Push(layer.Value ?? "~<NULL>~");
        }

        try
        {
            if (!layer.IsDefault)
            {
                settings.LayersSettings.Add(string.Join("|", propertyStack.Reverse()),
                    string.Join("",
                        (IEnumerable<char>)[BoolToChar(layer.ShowLayer), .. layer.ShowLevels.Select(BoolToChar)]));
            }

            foreach (var subLayer in layer.SubLayers)
            {
                WriteLogLevelFilterLayerSettings(subLayer, propertyStack, settings);
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(layer.Property))
            {
                propertyStack.Pop();
            }
        }
    }

    private void ApplyLogLevelFilterProperties(LogLevelFilterLayerModel layer, Stack<string> propertyStack,
        LogLevelFilterSettings settings)
    {
        if (!string.IsNullOrEmpty(layer.Property))
        {
            propertyStack.Push(layer.Value ?? "~<NULL>~");
        }

        try
        {
            if (settings.LayersSettings.TryGetValue(string.Join("|", propertyStack.Reverse()), out var boos))
            {
                if (boos.Length >= layer.ShowLevels.Count + 1)
                {
                    layer.ShowLayer = CharToBool(boos[0]);
                    for (int n = 0; n < layer.ShowLevels.Count; ++n)
                    {
                        layer[(LogEventLevel)n] = CharToBool(boos[n + 1]);
                    }
                }
            }

            foreach (var subLayer in layer.SubLayers)
            {
                ApplyLogLevelFilterProperties(subLayer, propertyStack, settings);
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(layer.Property))
            {
                propertyStack.Pop();
            }
        }
    }

    private char BoolToChar(bool? boo) => boo switch
    {
        true => '1',
        false => '0',
        _ => '-'
    };

    private bool? CharToBool(char ch) => ch switch
    {
        '1' => true,
        '0' => false,
        _ => null
    };
}