namespace CodeYesterday.Lovi.Models;

internal record PackageLicenseModel
{
    public required string PackageName { get; init; }

    public required string PackageVersion { get; init; }

    public string? PackageUrl { get; init; }

    public string? Copyright { get; init; }

    public string? Description { get; init; }

    public string? LicenseUrl { get; init; }

    public string? LicenseType { get; init; }

    public List<string> Authors { get; init; } = [];
}
