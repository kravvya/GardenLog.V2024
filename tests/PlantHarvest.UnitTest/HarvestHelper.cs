using GardenLog.SharedKernel.Enum;
using GardenLog.SharedKernel;
using PlantHarvest.Contract.Commands;
using PlantHarvest.Contract.Enum;
using PlantHarvest.Contract.ViewModels;
using PlantHarvest.Domain.HarvestAggregate;
using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Domain.HarvestAggregate.Events.Meta;
using PlantHarvest.Domain.WorkLogAggregate;
using PlantHarvest.Domain.WorkLogAggregate.Events;
using PlantHarvest.Domain.WorkLogAggregate.Events.Meta;

namespace PlantHarvest.UnitTest
{
    internal class HarvestHelper
    {
        public static string HARVEST_CYCLE_ID = "HarvestCycleId";
        public static string PLANT_HARVEST_CYCLE_ID = "PlantHarvestCycleId";
        public static string PLANT_TASK_ID = "PlantTaskId";
        public const string WORK_LOG_ID = "WorkLogId";

        //public static HarvestEvent GetPlantHarvestEvent(HarvestEventTriggerEnum trigger, PlantHarvest.Contract.Enum.PlantingMethodEnum plantingMethod, WorkLogReasonEnum taskType)
        //{
        //    var harvest = GetHarvestCycle();
        //    var plantHarvestId = harvest.AddPlantHarvestCycle(GetCommandToCreatePlantHarvestCycle(plantingMethod));
        //    harvest.AddPlantSchedule(GetCommandToCreateSchedule(plantHarvestId, taskType));

        //    var evt = new HarvestEvent(harvest, trigger, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, plantHarvestId));

        //    return evt;
        //}

        public static HarvestCycle GetHarvestCycle()
        {
            return HarvestCycle.Create(UserManagementHelper.USER_PROFILE_ID
                , "Test Harvest"
                , DateTime.Now.AddDays(1)
                , null
                , "Test Harvst Details"
                , UserManagementHelper.GARDEN_ID);

        }

        public static CreatePlantHarvestCycleCommand GetCommandToCreatePlantHarvestCycle(PlantHarvest.Contract.Enum.PlantingMethodEnum plantingMethod)
        {
            return new CreatePlantHarvestCycleCommand()
            {
                DesiredNumberOfPlants = 30,
                FirstHarvestDate = null,
                LastHarvestDate = null,
                GerminationDate = null,
                GerminationRate = null,
                HarvestCycleId = PLANT_HARVEST_CYCLE_ID,
                Notes = "Test PLant Harvest Cycle Note",
                NumberOfSeeds = 30,
                NumberOfTransplants = null,
                PlantGrowthInstructionId = PlantsHelper.GROW_INSTRUCTION_ID,
                PlantGrowthInstructionName = "Test Grow Instruction Name",
                PlantId = PlantsHelper.PLANT_ID,
                PlantingMethod = plantingMethod,
                PlantName = "Test Plant",
                PlantVarietyId = PlantsHelper.PLANT_VARIETY_ID,
                PlantVarietyName = "Test Variety Name",
                SeedingDate = null,
                SeedVendorId = "SeedVendorId",
                SeedVendorName = "Good Seeds",
                TotalItems = 0,
                TotalWeightInPounds = 0,
                TransplantDate = null
            };
        }

        public static PlantHarvestCycleViewModel GetPlantHarvestCycleViewModel(PlantHarvest.Contract.Enum.PlantingMethodEnum plantingMethod, PlantScheduleViewModel schedule)
        {
            return new PlantHarvestCycleViewModel()
            {
                PlantHarvestCycleId = PLANT_HARVEST_CYCLE_ID,
                DesiredNumberOfPlants = 30,
                FirstHarvestDate = null,
                LastHarvestDate = null,
                GerminationDate = null,
                GerminationRate = null,
                HarvestCycleId = HARVEST_CYCLE_ID,
                Notes = "Test PLant Harvest Cycle Note",
                NumberOfSeeds = 30,
                NumberOfTransplants = null,
                PlantGrowthInstructionId = PlantsHelper.GROW_INSTRUCTION_ID,
                PlantGrowthInstructionName = "Test Grow Instruction Name",
                PlantId = PlantsHelper.PLANT_ID,
                PlantingMethod = plantingMethod,
                PlantName = "Test Plant",
                PlantVarietyId = PlantsHelper.PLANT_VARIETY_ID,
                PlantVarietyName = "Test Variety Name",
                SeedingDate = null,
                SeedVendorId = "SeedVendorId",
                SeedVendorName = "Good Seeds",
                TotalItems = 0,
                TotalWeightInPounds = 0,
                TransplantDate = null,
                PlantCalendar = new List<PlantScheduleViewModel>() { schedule }
            };
        }

        #region Plant Task
        public static PlantTaskViewModel GetPlantTaskViewModel(string plantTaskId, string plantHarvestCycleId, WorkLogReasonEnum taskType, string title ="")
        {
            return new PlantTaskViewModel()
            {
                PlantTaskId = plantTaskId,
                PlantHarvestCycleId = plantHarvestCycleId,
                TargetDateStart = DateTime.Now.AddDays(1),
                TargetDateEnd = DateTime.Now.AddDays(7),
                IsSystemGenerated = true,
                Notes = "Test SChedule",
                Type = taskType,
                Title = title
            };
        }
        #endregion

        #region Plant Schedules
        public static CreatePlantScheduleCommand GetCommandToCreateSchedule(string plantHarvestCycleId, WorkLogReasonEnum taskType)
        {
            return new CreatePlantScheduleCommand()
            {
                PlantHarvestCycleId = plantHarvestCycleId,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(7),
                IsSystemGenerated = true,
                Notes = "Test SChedule",
                TaskType = taskType
            };
        }

        public static PlantScheduleViewModel GetPlantScheduleViewModel(string plantHarvestCycleId, WorkLogReasonEnum taskType, DateTime startDate)
        {
            return new PlantScheduleViewModel()
            {
                PlantHarvestCycleId = plantHarvestCycleId,
                StartDate = startDate,
                EndDate = startDate.AddDays(7),
                IsSystemGenerated = true,
                Notes = "Test Schedule",
                TaskType = taskType
            };
        }
        #endregion

        #region WorkLog
        public static WorkLogEvent GetWorkLogEvent(WorkLogEventTriggerEnum trigger, string harvestId, string harvestName, string plantHarvestCycleId, string plantName, WorkLogReasonEnum taskType)
        {
            var workLog = GetWorkLog(harvestId, harvestName, plantHarvestCycleId, plantName, taskType);

            var evt = new WorkLogEvent(workLog, trigger, new RelatedEntity(RelatedEntityTypEnum.WorkLog, WORK_LOG_ID, string.Empty));

            return evt;
        }

        public static WorkLog GetWorkLog(string harvestId, string harvestName, string plantHarvestCyceld, string plantName, WorkLogReasonEnum taskType)
        {
            var relatedEntities = new List<RelatedEntity>();

            relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, harvestId, harvestName));
            relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.PlantHarvestCycle, plantHarvestCyceld, plantName));

            return WorkLog.Create(
                "Test WorkLog Log",
                DateTime.Now,
                taskType,
                UserManagementHelper.USER_PROFILE_ID,
                relatedEntities);

        }
        #endregion
    }
}
