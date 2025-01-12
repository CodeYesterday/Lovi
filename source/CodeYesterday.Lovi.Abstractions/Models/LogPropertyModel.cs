namespace CodeYesterday.Lovi.Models;

[PublicAPI]
public class LogPropertyModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public List<PropertyType> PropertyTypes { get; } = new();
}

public enum PropertyType
{
    None,
    String,
    Float,
    Integer,
    Unsigned,
    List,
    Map,
    Object
}

