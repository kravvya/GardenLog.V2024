global using PlantHarvest.Contract.Commands;
global using PlantHarvest.Domain.HarvestAggregate;
global using PlantHarvest.Contract.ViewModels;
global using PlantHarvest.Api.CommandHandlers;
global using PlantHarvest.Api.QueryHandlers;
global using GardenLog.SharedInfrastructure.Extensions;
global using PlantHarvest.Api.Schedules;
global using PlantHarvest.Domain.PlantTaskAggregate;
global using PlantHarvest.Domain.WorkLogAggregate;
global using PlantHarvest.Contract;
global using PlantCatalog.Contract.ViewModels;
global using PlantHarvest.Contract.Enum;
global using UserManagement.Contract.ViewModels;
global using Plant = PlantCatalog.Contract.Enum;
global using PlantHarvest.Domain.HarvestAggregate.Events.Meta;

global using MediatR;
public static class GlobalConstants
{
    public const string PLANTCATALOG_API = "PlantCatalog.Api";
    public const int DEFAULT_FertilizeFrequencyForSeedlingsInWeeks = 2;
    public const int DEFAULT_FertilizeFrequencyInWeeks = 4;
    public const int DEFAULT_HardenOffPeriodInDays = 7;
}