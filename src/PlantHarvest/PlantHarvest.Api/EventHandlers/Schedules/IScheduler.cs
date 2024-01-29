namespace PlantHarvest.Api.Schedules;

public interface IScheduler
{
    public bool CanSchedule(PlantGrowInstructionViewModel growInstruction);

    public CreatePlantScheduleCommand? Schedule(PlantHarvestCycle plantHarvest, PlantGrowInstructionViewModel growInstruction, GardenViewModel garden, int? daysToMaturityMin, int? daysToMaturityMax);
}
