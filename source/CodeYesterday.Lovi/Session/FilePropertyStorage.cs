using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CodeYesterday.Lovi.Session;

/// <summary>
/// JSON file implementation of the <see cref="IPropertyStorage"/> interface.
/// </summary>
internal class FilePropertyStorage : IPropertyStorage
{
    private readonly ConcurrentDictionary<string, object> _properties = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SaveProperties(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken)
    {
        var path = GetFilePath(dataDirectory);
        await using var fileStream = File.Create(path);

        var data = new Dictionary<string, JsonNode>();
        foreach (var property in _properties)
        {
            var value = property.Value switch
            {
                string s => JsonValue.Create(s),
                bool boo => JsonValue.Create(boo),
                sbyte sb => JsonValue.Create(sb),
                byte by => JsonValue.Create(by),
                short sh => JsonValue.Create(sh),
                ushort us => JsonValue.Create(us),
                int i => JsonValue.Create(i),
                uint ui => JsonValue.Create(ui),
                long l => JsonValue.Create(l),
                ulong ul => JsonValue.Create(ul),
                float f => JsonValue.Create(f),
                double d => JsonValue.Create(d),
                DateTime dt => JsonValue.Create(dt),
                DateTimeOffset dto => JsonValue.Create(dto),
                _ => ToJsonNode(property.Value)
            };

            if (value is null)
            {
                Debug.Assert(false, $"Can not serialize property value {property.Value}");
                // TODO: Log
                continue;
            }
            data.Add(property.Key, value);
        }

        await JsonSerializer
            .SerializeAsync(fileStream, data, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    private class JsonObjectWrapper
    {
        public required string Type { get; set; }

        public required object Data { get; set; }
    }

    private JsonNode? ToJsonNode(object obj)
    {
        var data = obj;
        var type = obj.GetType();
        var typeName = $"{type.FullName}, {type.Assembly.GetName().Name}";

        return JsonSerializer.SerializeToNode(new JsonObjectWrapper
        {
            Type = typeName,
            Data = data
        });
    }

    public async Task LoadProperties(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken)
    {
        try
        {
            var path = GetFilePath(dataDirectory);
            await using var fileStream = File.OpenRead(path);

            var data = await JsonSerializer
                .DeserializeAsync<Dictionary<string, JsonNode>>(fileStream, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            _properties.Clear();

            if (data is null) return;

            foreach (var property in data)
            {
                if (property.Value is JsonValue jv)
                {
                    switch (jv.GetValueKind())
                    {
                        case JsonValueKind.String:
                            _properties.TryAdd(property.Key, jv.ToString());
                            break;
                        case JsonValueKind.Number:
                            var str = jv.ToString();
                            if (str.Contains('.'))
                            {
                                _properties.TryAdd(property.Key, jv.GetValue<double>());
                            }
                            else if (str.Contains('-'))
                            {
                                _properties.TryAdd(property.Key, jv.GetValue<long>());
                            }
                            else
                            {
                                _properties.TryAdd(property.Key, jv.GetValue<ulong>());
                            }
                            break;
                        case JsonValueKind.True:
                            _properties.TryAdd(property.Key, true);
                            break;
                        case JsonValueKind.False:
                            _properties.TryAdd(property.Key, false);
                            break;
                        default:
                            // ignore value
                            break;
                    }
                }
                else if (property.Value is JsonObject jo)
                {
                    try
                    {
                        if (jo.TryGetPropertyValue(nameof(JsonObjectWrapper.Type), out var typeNode) && typeNode is not null &&
                            jo.TryGetPropertyValue(nameof(JsonObjectWrapper.Data), out var typeDataNode) && typeDataNode is not null)
                        {
                            var typeName = typeNode.ToString();
                            var type = Type.GetType(typeName);
                            var obj = type is null ? null : typeDataNode.Deserialize(type);
                            if (obj is not null)
                            {
                                _properties.TryAdd(property.Key, obj);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // TODO: Log
                    }
                }
            }
        }
        catch (Exception)
        {
            // TODO: Add logging.
        }
    }

    public void SetPropertyValue<TValue>(string id, TValue value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        _properties.AddOrUpdate(id, _ => value, (_, _) => value);
    }

    public TValue GetPropertyValue<TValue>(string id, TValue defaultValue)
    {
        ArgumentNullException.ThrowIfNull(defaultValue, nameof(defaultValue));

        return TryGetPropertyValue(id, out TValue? value) ? value : defaultValue;
    }

    public bool TryGetPropertyValue<TValue>(string id, [NotNullWhen(true)] out TValue? value)
    {
        if (_properties.TryGetValue(id, out var vv))
        {
            try
            {
                if (typeof(TValue) == typeof(DateTimeOffset) && vv is string str)
                {
                    value = (TValue)(object)DateTimeOffset.Parse(str, CultureInfo.InvariantCulture);
                }
                else
                {
                    value = (TValue)Convert.ChangeType(vv, typeof(TValue));
                }
                return true;
            }
            catch (Exception)
            {
                // TODO: Log
            }
        }

        value = default;
        return false;
    }

    public void RemovePropertyValue(string id)
    {
        _properties.TryRemove(id, out _);
    }

    public bool PropertyValueExists(string id)
    {
        return _properties.ContainsKey(id);
    }

    private static string GetFilePath(string dataDirectory)
    {
        return Path.Combine(dataDirectory, "Properties.json");
    }
}
