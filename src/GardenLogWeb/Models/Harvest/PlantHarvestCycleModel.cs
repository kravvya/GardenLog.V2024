namespace GardenLogWeb.Models.Harvest;

public record PlantHarvestCycleModel : PlantHarvestCycleViewModel
{
    public new List<GardenBedPlantHarvestCycleModel> GardenBedLayout { get; set; } = new();

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

    public double GetPlantsPerFoot()
    {
        if(PlantsPerFoot.HasValue) return PlantsPerFoot.Value;

        //default to 1in
        if (!SpacingInInches.HasValue) return 144;

        switch (SpacingInInches)
        {
            case 1:
                return 144;
            case 2:
                return 36;
            case 3:
                return 16;
            case 4:
                return 9;
            case 5:
            case 6:
                return 4;
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
                return 1;
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
                return 0.5;
            default:
                return 0.25;
        }
    }

}
