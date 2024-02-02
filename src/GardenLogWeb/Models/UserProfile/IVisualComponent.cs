namespace GardenLogWeb.Models;

public interface IVisualComponent
{
    public string Id { get;  }
    public string Name { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public double Rotate { get; set; }
    public string? CssClass { get; }

    public double GetHeightInPixels();

    public double GetWidthInPixels();

    public void IncreaseLengthByPixels(double pixels);

    public void IncreaseWidthByPixels(double pixels);
    void MoveDown(int units);
    void MoveLeft(int units);
    void MoveRight(int units);
    void MoveUp(int units);
    void RotateBy(double rotate);
}

