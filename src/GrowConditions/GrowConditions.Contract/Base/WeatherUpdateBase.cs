
using Newtonsoft.Json;

namespace GrowConditions.Contract.Base;

public abstract record WeatherUpdateBase
{
    public string GardenId { get; set; } = string.Empty;
    public string GardenName { get; set; }=string.Empty;

    public string UserProfileId { get; set; } = string.Empty;

    public Location? Location { get; set; }
    public List<OpenWeatherCondition>? WeatherConditions { get; set; }

    public MainCondition? Main { get; set; }

    public decimal Visibility { get; set; }

    public Wind? Wind { get; set; }

    public Clouds? Clouds { get; set; }

    public DateTime UpdatedDateLocal { get; set; }
    public DateTime UpdatedDateUtc { get; set; }

    public Daylight? Daylight { get; set; }

    public int SecondsFromUtc { get; set; }

    public Rain? Rain { get; set; }
    public Snow? Snow { get; set; }
    
}

public class Location
{
    public Coordinate? Coord { get; set; }
    public string Country { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
}

public class Daylight
{

    public DateTime SunriseUtc { get; set; }

    public DateTime SunsetUtc { get; set; }
    public DateTime SunriseLocal { get; set; }

    public DateTime SunsetLocal { get; set; }
}

public class WeatherCondition
{
    public string WeatherConditionId { get; set; } = string.Empty;
    public string Main { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
public class Clouds
{
    public int All { get; set; }
}

public class Rain
{
    public static Rain Empty()
    {
        return new Rain();
    }
    public decimal? LastHour { get; set; }
    public decimal? Last3Hours { get; set; }

    public bool IsEmpty()
    {
        return !LastHour.HasValue && !Last3Hours.HasValue;
    }
}

public class Snow
{
    public static Snow Empty()
    {
        return new Snow();
    }
    public decimal? LastHour { get; set; }
    public decimal? Last3Hours { get; set; }
    public bool IsEmpty()
    {
        return !LastHour.HasValue && !Last3Hours.HasValue;
    }

}
public class Wind
{
    public decimal Speed { get; set; }
    public decimal Deg { get; set; }
    public decimal Gust { get; set; }
}
public class MainCondition
{
    public decimal Temp { get; set; }
    public decimal Feels_like { get; set; }
    public decimal Pressure { get; set; }
    public decimal Humidity { get; set; }
    public decimal Temp_min { get; set; }
    public decimal Temp_max { get; set; }
}

public class Coordinate
{
    public decimal Lat { get; set; }
    public decimal Lon { get; set; }
}

public class OpenWeatherCondition
{
    [JsonProperty("id")]
    public int WeatherConditionId { get; set; }
    public string Main { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}