using GardenLogWeb.Pages.GardenLayout.Components;
using System.Reflection.Metadata.Ecma335;

namespace GardenLogWeb.Models.UserProfile;

public record GardenBedModel : GardenBedViewModel, IVisualComponent
{
    public string? CssClass { get => this.Type.ToString(); }
    public string Id { get => this.GardenBedId; }

    public double GetHeightInPixels()
    {
        return this.Length / 12 * GardenPlanSettings.TickFootHeight;
    }

    public double GetWidthInPixels()
    {
        return this.Width / 12 * GardenPlanSettings.TickFootWidth;
    }

    public void IncreaseLengthByPixels(double pixels)
    {
        Length += 12 * pixels / GardenPlanSettings.TickFootHeight;
        if (Length <= 0) Length = 1;
    }

    public void IncreaseWidthByPixels(double pixels)
    {
        Width += 12 * pixels / GardenPlanSettings.TickFootWidth;
        if (Width <= 0) Width = 1;
    }

    public string GetLengthDisplay()
    {
        double feet = Length / 12;
        int feetInt = (int)feet;
        double inchesRemainder = Length % 12;

        if(inchesRemainder> 0)
        {
            return ($"{feetInt}' {inchesRemainder}\"");
        }
        else
        {
            return ($"{feetInt}'");
        }
        
    }

    public string GetWidthDisplay()
    {

        double feet = Width / 12;
        int feetInt = (int)feet;
        double inchesRemainder = Width % 12;

        if (inchesRemainder > 0)
        {
            return ($"{feetInt}' {inchesRemainder}\"");
        }
        else
        {
            return ($"{feetInt}'");
        }
    }

    public void MoveUp(int units)
    {
        Y -= units;
    }

    public void MoveDown(int units)
    {
        Y += units;
    }

    public void MoveLeft(int units)
    {
        X -= units;
    }

    public void MoveRight(int units)
    {
        X += units;
    }

    public void RotateBy(double rotate)
    {
        Rotate += rotate;
        if (Rotate > 360)
        {
            Rotate -= 360;
        }
        if(Rotate == 360)
        {
            Rotate= 0;
        }
    }
}