namespace GardenLog.Mcp.Application.Tools.Models;

/// <summary>
/// Simplified garden details optimized for AI analysis.
/// Excludes visual layout fields and user IDs.
/// </summary>
public record GardenDetailsToolResult
{
    public string GardenId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? City { get; init; }
    public string? StateCode { get; init; }
    public string? Notes { get; init; }
    public DateTime? LastFrostDate { get; init; }
    public DateTime? FirstFrostDate { get; init; }
    public DateTime? WarmSoilDate { get; init; }
    public IReadOnlyCollection<GardenBedToolResult> GardenBeds { get; init; } = Array.Empty<GardenBedToolResult>();
}

/// <summary>
/// Simplified garden bed details optimized for AI analysis.
/// Excludes visual coordinates (x, y, rotate, rowNumber).
/// </summary>
public record GardenBedToolResult
{
    public string GardenBedId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string Type { get; init; } = string.Empty;  // InGroundBed, RaisedBed, etc.
    public double Length { get; init; }  // Inches
    public double Width { get; init; }   // Inches
}