using Serilog.Events;

namespace CodeYesterday.Lovi.Models;

[PublicAPI]
public class LogItemModel
{
    public required long Id { get; init; }

    public required int FileId { get; init; }

    public required LogEvent LogEvent { get; init; }

    public bool IsBookmarked { get; set; }

    public string RenderedMessage => LogEvent.RenderMessage();

    public string? ExceptionText => LogEvent.Exception?.ToString();

    public PropertyType GetPropertyType(string name)
    {
        if (LogEvent.Properties.TryGetValue(name, out var p))
        {
            return p switch
            {
                ScalarValue scalarValue => scalarValue.Value switch
                {
                    null => PropertyType.None,
                    string => PropertyType.String,
                    float or double => PropertyType.Float,
                    short or int or long => PropertyType.Integer,
                    byte or ushort or int or ulong => PropertyType.Unsigned,
                    _ => PropertyType.String
                },
                SequenceValue => PropertyType.List,
                DictionaryValue => PropertyType.Map,
                StructureValue => PropertyType.Object,
                _ => PropertyType.String
            };
        }

        return PropertyType.None;
    }

    public object? this[string name]
    {
        get
        {
            bool isTyped = name.Length > 2 && name[1] == ':';
            var type = isTyped ? name[0] : ' ';
            if (isTyped) name = name.Substring(2);

            if (LogEvent.Properties.TryGetValue(name, out var p))
            {
                try
                {
                    return type switch
                    {
                        's' => p switch
                        {
                            ScalarValue scalarValue => scalarValue.Value?.ToString(),
                            _ => null
                        },
                        'f' => p switch
                        {
                            ScalarValue scalarValue => scalarValue.Value is (float or double)
                                ? Convert.ToDouble(scalarValue.Value)
                                : null,
                            _ => null,
                        },
                        'i' => p switch
                        {
                            ScalarValue scalarValue => scalarValue.Value is (short or int or long)
                                ? Convert.ToInt64(scalarValue.Value)
                                : null,
                            _ => null,
                        },
                        'u' => p switch
                        {
                            ScalarValue scalarValue => scalarValue.Value is (byte or ushort or uint or ulong)
                                ? Convert.ToUInt64(scalarValue.Value)
                                : null,
                            _ => null,
                        },
                        'l' => p switch
                        {
                            SequenceValue sequenceValue => sequenceValue.Elements,
                            _ => null,
                        },
                        'm' => p switch
                        {
                            DictionaryValue dictionaryValue => dictionaryValue.Elements,
                            _ => null,
                        },
                        'o' => p switch
                        {
                            StructureValue structureValue => structureValue,
                            _ => null,
                        },
                        _ => p switch
                        {
                            ScalarValue scalarValue => scalarValue.Value?.ToString(),
                            _ => p.ToString()
                        }
                    };
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the scalar string value of the property.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>Returns the scalar string value or <see langword="null"/> if the item does not have this property or the property is not a scalar string value</returns>.
    /// <remarks>This is the same as <c>this[s:<paramref name="name"/>]</c>, but a little bit better in performance.</remarks>
    public string? GetPropertyScalarStringValue(string name)
    {
        if (LogEvent.Properties.TryGetValue(name, out var p))
        {
            return p switch
            {
                ScalarValue scalarValue => scalarValue.Value?.ToString(),
                _ => null
            };
        }

        return null;
    }
}