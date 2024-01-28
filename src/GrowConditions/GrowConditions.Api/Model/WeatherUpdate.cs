using GardenLog.SharedKernel.Interfaces;
using GrowConditions.Contract.Base;

namespace GrowConditions.Api.Model;

public record WeatherUpdate: WeatherUpdateBase, IEntity
{
    public string Id { get; set; } = string.Empty;
}
