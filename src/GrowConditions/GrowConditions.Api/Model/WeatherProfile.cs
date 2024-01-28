using GrowConditions.Contract.Base;

namespace GrowConditions.Api.Model;

public class  WeatherProfile: Profile
{
    public WeatherProfile()
    {
        CreateMap<OpenWeather, WeatherUpdate>();
        CreateMap<OpenWeatherCondition, WeatherCondition>();
        CreateMap<OpenWeatherRain, Rain>();
        CreateMap<OpenWeatherSnow, Snow>();
        CreateMap<OpenWeatherWind, Wind>();
        CreateMap<OpenWeatherClouds, Clouds>();
        CreateMap<OpenWeatherMainCondition, MainCondition>();
        CreateMap<OpenWeatherSystem, Daylight>();
    }
}
