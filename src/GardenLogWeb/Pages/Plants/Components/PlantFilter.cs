

namespace GardenLogWeb.Pages.Plants.Components;

public class PlantFilter
{

    public List<CheckableEnum> PlantLifecycleEnums { get; set; } = new();
    public List<CheckableEnum> PlantTypeEnums { get; set; } = new();
    public List<CheckableEnum> LightRequirementEnums { get; set; } = new();
    public List<CheckableEnum> MoistureRequirementEnums { get; set; } = new();

    public PlantLifecycleEnum PlantLifecycle { get; set; }

    public PlantFilter(IVerifyService verifyService)
    {
        var plantLifeCyclesCodes = verifyService.GetCodeList<PlantLifecycleEnum>(true);
        var plantTypeCodes = verifyService.GetCodeList<PlantTypeEnum>(true);
        var lightRequirementCodes = verifyService.GetCodeList<LightRequirementEnum>(true);
        var moistureRequirementCodes = verifyService.GetCodeList<MoistureRequirementEnum>(true);

        foreach (var code in plantLifeCyclesCodes)
        {
            PlantLifecycleEnums.Add(new CheckableEnum(code));
        }

        foreach (var code in plantTypeCodes)
        {
            PlantTypeEnums.Add(new CheckableEnum(code));
        }

        foreach (var code in lightRequirementCodes)
        {
            LightRequirementEnums.Add(new CheckableEnum(code));
        }

        foreach (var code in moistureRequirementCodes)
        {
            MoistureRequirementEnums.Add(new CheckableEnum(code));
        }
    }

}
