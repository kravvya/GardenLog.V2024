using GardenLog.SharedKernel;
using GrowConditions.Api.Model.Meta;

namespace GrowConditions.Api.Model;

public record WeatherEvent : BaseDomainEvent
{
    public WeatherUpdate? Weather { get; init; }
    public string Trigger { get; set; } = string.Empty;
    public Meta.TriggerEntity? TriggerEntity { get; set; }
    public string UserProfileId { get { return Weather!.UserProfileId; } init { } }
    private WeatherEvent() { }

    public WeatherEvent(WeatherUpdate weather)
    {
        Weather = weather;
        Trigger = "WeatherUpdate";
        TriggerEntity = new(EntityTypeEnum.WeatherUpdate, Weather.Id);
    }
}
