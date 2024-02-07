using Newtonsoft.Json;

namespace GrowConditions.Api.Model;

public class NationalWeatherPointData
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public Geometry? Geometry { get; set; }
    public required PointDataProperties Properties { get; set; }
}

public class PointDataProperties
{
    [JsonProperty("@id")]
    public required string Id { get; set; }
    [JsonProperty("@type")]
    public required string Type { get; set; }
    public required string Cwa { get; set; }
    public required string ForecastOffice { get; set; }
    public required string GridId { get; set; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public string? Forecast { get; set; }
    public string? ForecastHourly { get; set; }
    public string? ForecastGridData { get; set; }
    public string? ObservationStations { get; set; }
    public Geometry? RelativeLocation { get; set; }
    public string? ForecastZone { get; set; }
    public string? County { get; set; }
    public string? FireWeatherZone { get; set; }
    public required string TimeZone { get; set; }
    public string? RadarStation { get; set; }
}

public class Geometry
{
    public required string Type { get; set; }
    public required List<double> Coordinates { get; set; }
}