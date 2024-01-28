using GrowConditions.Contract.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GrowConditions.Api.Model;

public class OpenWeather
{
    public Coordinate? Coord { get; set; }

    [JsonProperty("weather")]
    public List<OpenWeatherCondition>? WeatherConditions { get; set; }

    public MainCondition? Main { get; set; }

    public decimal Visibility { get; set; }

    public OpenWeatherWind? Wind { get; set; }

    public OpenWeatherClouds? Clouds { get; set; }


    [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime Dt { get; set; }

    public OpenWeatherSystem? Sys { get; set; }

    [JsonProperty("timezone")]
    public int SecondsFromUtc { get; set; }


    [JsonProperty("id")]
    public int CityId { get; set; }
    [JsonProperty("name")]
    public string CityName { get; set; } = string.Empty;

    public OpenWeatherRain? Rain { get; set; }
    public OpenWeatherSnow? Snow { get; set; }
}



public class OpenWeatherMainCondition
{
    public decimal Temp { get; set; }
    public decimal Feels_like { get; set; }
    public decimal Pressure { get; set; }
    public decimal Humidity { get; set; }
    public decimal Temp_min { get; set; }
    public decimal Temp_max { get; set; }
}

public class OpenWeatherWind
{
    public decimal Speed { get; set; }
    public decimal Deg { get; set; }
    public decimal Gust { get; set; }
}

public class OpenWeatherRain
{
    [JsonProperty("1h")]
    public decimal LastHour { get; set; }
    [JsonProperty("3h")]
    public decimal Last3Hours { get; set; }

}
public class OpenWeatherSnow
{
    [JsonProperty("1h")]
    public decimal LastHour { get; set; }
    [JsonProperty("3h")]
    public decimal Last3Hours { get; set; }

}

public class OpenWeatherClouds
{
    public int All { get; set; }
}

public class OpenWeatherSystem
{
    public int Type { get; set; }
    public int Id { get; set; }
    public string Country { get; set; } = string.Empty;
     [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime Sunrise { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime Sunset { get; set; }
}

