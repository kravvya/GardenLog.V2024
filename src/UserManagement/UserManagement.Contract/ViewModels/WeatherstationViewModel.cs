namespace UserManagement.Contract.ViewModels;

public record WeatherstationViewModel: WeatherstationBase
{
    public string WeatherstationId { get; set; } = string.Empty;
}
