namespace CodeYesterday.Lovi.Session;

public record LogDataStats
{
    public static readonly LogDataStats Empty = new()
    {
        LogItemCount = 0,
        FirstItemTimestamp = DateTimeOffset.MinValue,
        LastItemTimestamp = DateTimeOffset.MinValue
    };

    public required int LogItemCount { get; init; }

    public required DateTimeOffset FirstItemTimestamp { get; init; }

    public required DateTimeOffset LastItemTimestamp { get; init; }
}