using MongoDB.Driver.Linq;

namespace PlantHarvest.Api.Schedules;

public abstract class SchedulerBase
{
    public DateTime? GetStartDateBasedOnWeatherCondition(Plant.WeatherConditionEnum weatherCondition, int weeksAhead, GardenViewModel garden)
    {
        DateTime? startDate = null;
        int year = DateTime.Now.Year;

        switch (weatherCondition)
        {
            case Plant.WeatherConditionEnum.BeforeLastFrost:
                DateTime lastFrost = new DateTime(year, garden.LastFrostDate.Month, garden.LastFrostDate.Day);
                startDate = lastFrost.AddDays(-7 * weeksAhead);
                break;

            case Plant.WeatherConditionEnum.BeforeFirstFrost:
                DateTime firstFrost = new DateTime(year, garden.FirstFrostDate.Month, garden.FirstFrostDate.Day);
                startDate = firstFrost.AddDays(-7 * weeksAhead);
                break;

            case Plant.WeatherConditionEnum.EarlySpring:
                lastFrost = new DateTime(year, garden.LastFrostDate.Month, garden.LastFrostDate.Day);
                startDate = lastFrost.AddDays(-7 * 4);
                break;
            case Plant.WeatherConditionEnum.AfterDangerOfFrost:
                lastFrost = new DateTime(year, garden.LastFrostDate.Month, garden.LastFrostDate.Day);
                startDate = lastFrost.AddDays(7 * weeksAhead);

                break;
            case Plant.WeatherConditionEnum.MidSummer:
                lastFrost = new DateTime(year, garden.LastFrostDate.Month, garden.LastFrostDate.Day);
                firstFrost = new DateTime(year, garden.FirstFrostDate.Month, garden.FirstFrostDate.Day);

                double growDays = (firstFrost - lastFrost).TotalDays;
                startDate = lastFrost.AddDays(growDays / 2).AddDays(7 * weeksAhead);
                break;
            case Plant.WeatherConditionEnum.WarmSoil:
                DateTime warmSoil = new DateTime(year, garden.WarmSoilDate.Month, garden.WarmSoilDate.Day);
                startDate = warmSoil.AddDays(-7 * weeksAhead);
                break;
        }

        return startDate;
    }
}
