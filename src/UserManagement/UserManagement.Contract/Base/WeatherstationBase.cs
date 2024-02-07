namespace UserManagement.Contract.Base;

public abstract record WeatherstationBase
{
    public string ForecastOffice { get; set; } = string.Empty;
    public double GridX { get; set; }
    public double GridY { get; set; }
    public string Timezone { get; set; } = string.Empty;
}
