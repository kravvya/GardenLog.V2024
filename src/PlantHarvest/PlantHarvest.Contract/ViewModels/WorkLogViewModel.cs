namespace PlantHarvest.Contract.ViewModels;

public record WorkLogViewModel: WorkLogBase
{
    public string WorkLogId { get; set; } = string.Empty;
}


public class WorkLogViewModelValidator : WorkLogValidator<WorkLogViewModel>
{
    public WorkLogViewModelValidator()
    {
    }
}