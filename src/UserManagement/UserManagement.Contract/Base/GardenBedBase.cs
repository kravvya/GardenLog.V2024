namespace UserManagement.Contract.Base;

public abstract record GardenBedBase
{
    public string GardenId { get; set; }=string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Notes { get; set; }= string.Empty;
    public int? RowNumber { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public GardenBedTypeEnum Type { get; set; }
    public double Rotate { get; set; }

}


