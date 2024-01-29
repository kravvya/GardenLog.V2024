﻿using MongoDB.Driver.Linq;
using PlantCatalog.Contract.Enum;
using System.Text;
using GardenLog.SharedInfrastructure.Extensions;

namespace PlantHarvest.Api.Schedules;

public class IndoorSowScheduler : SchedulerBase, IScheduler
{
    public bool CanSchedule(PlantGrowInstructionViewModel growInstruction)
    {
        return growInstruction.PlantingMethod == Plant.PlantingMethodEnum.SeedIndoors;
    }

    public CreatePlantScheduleCommand? Schedule(PlantHarvestCycle plantHarvest, PlantGrowInstructionViewModel growInstruction, GardenViewModel garden,int? daysToMaturityMin, int? daysToMaturityMax)
    {
        int weeksAhead = growInstruction.StartSeedWeeksAheadOfWeatherCondition.HasValue ? growInstruction.StartSeedWeeksAheadOfWeatherCondition.Value : 0;

        DateTime? startDate = GetStartDateBasedOnWeatherCondition(growInstruction.StartSeedAheadOfWeatherCondition, weeksAhead, garden);
        DateTime endDate;

        if (startDate.HasValue)
        {
            StringBuilder sb = new();
            if (plantHarvest.DesiredNumberOfPlants.HasValue) sb.Append($"Desired number of plants: {plantHarvest.DesiredNumberOfPlants}. " );
            if (!string.IsNullOrEmpty(plantHarvest.SeedCompanyName)) sb.Append($"Seeds from {plantHarvest.SeedCompanyName}. ");
            if (growInstruction.FertilizerForSeedlings != FertilizerEnum.Unspecified) sb.Append($"Fertilize with {growInstruction.FertilizerForSeedlings.GetDescription()}. ");
            sb.Append(growInstruction.StartSeedInstructions.ToString());

           endDate = growInstruction.StartSeedWeeksRange.HasValue ? startDate.Value.AddDays(7 * growInstruction.StartSeedWeeksRange.Value) : startDate.Value;

            return new CreatePlantScheduleCommand()
            {
                TaskType = WorkLogReasonEnum.SowIndoors,
                StartDate = startDate.Value,
                EndDate = endDate,
                IsSystemGenerated = true,
                Notes = sb.ToString()
            };
        }

        return null;
    }
}
