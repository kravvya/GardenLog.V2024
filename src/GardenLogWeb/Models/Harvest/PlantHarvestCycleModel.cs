namespace GardenLogWeb.Models.Harvest;

public record PlantHarvestCycleModel : PlantHarvestCycleViewModel
{
    public new List<GardenBedPlantHarvestCycleModel> GardenBedLayout { get; set; } = [];

    public string ImageFileName { get; set; } = string.Empty;
    public string ImageLabel { get; set; } = string.Empty;
    public List<ImageViewModel>? Images { get; set; }

    public string GetPlantName()
    {
        if (string.IsNullOrEmpty(PlantVarietyName))
        {
            return PlantName;
        }
        else
        {
            return $"{PlantName} - {PlantVarietyName}";
        }
    }

    public int NumberOfPlantsInGardenBedLayout => GardenBedLayout.Sum(b => b.NumberOfPlants);

    public int NumberOfPlantsLeftToAddToGardenBedLayout
    {
        get
        {
            var plantsLeft = Convert.ToInt32(DesiredNumberOfPlants - NumberOfPlantsInGardenBedLayout);
            return plantsLeft > 0 ? plantsLeft : 0;
        }
    }

    public string GetPlantingDate()
    {
        if (PlantCalendar == null) return string.Empty;

        var schedule = PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.Plant || s.TaskType == WorkLogReasonEnum.TransplantOutside || s.TaskType == WorkLogReasonEnum.SowOutside);
        if (schedule == null) return string.Empty;

        return schedule.StartDate.ToShortDateString();
    }

    public DateTime? GetSeasonStartDate()
    {
        if (PlantCalendar == null) return null;

        var schedule = PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.Plant || s.TaskType == WorkLogReasonEnum.TransplantOutside || s.TaskType == WorkLogReasonEnum.SowOutside || s.TaskType == WorkLogReasonEnum.SowIndoors);
        if (schedule == null) return null;

        return schedule.StartDate;
    }

    public double GetPlantsPerFoot()
    {
        if(PlantsPerFoot.HasValue) return PlantsPerFoot.Value;

        //default to 1in
        if (!SpacingInInches.HasValue) return 144;

        return SpacingInInches switch
        {
            1 => 144,
            2 => 36,
            3 => 16,
            4 => 9,
            5 or 6 => 4,
            7 or 8 or 9 or 10 or 11 or 12 or 13 or 14 => 1,
            15 or 16 or 17 or 18 or 19 or 20 or 21 or 22 or 23 or 24 => 0.5,
            _ => 0.25,
        };
    }

}
