using GardenLogWeb.Models.Enums;

namespace GardenLogWeb.Pages.HarvestGardenLayout.Components;

public record SelectorSide(double OriginalAngle, ComponentChanges Changes)
{
    public double TotalAngle { get; set; }
    public string CssClass { get; set; } = string.Empty;
    public string Axis { get; set; } = string.Empty;
    public bool LessIsMore { get; set; } = false;

}

public record SelectorSides
{
    public List<SelectorSide> Sides { get; set; } = new();
    public bool LessIsMoreX { get; set; } = false;
    public bool LessIsMoreY { get; set; } = false;
    public bool FlipAxesForMove { get; set; } = false;
    public SelectorSides(double rotate)
    {
        Sides = new() {
            new SelectorSide(270, ComponentChanges.Left),
            new SelectorSide(180, ComponentChanges.Bottom),
            new SelectorSide(90, ComponentChanges.Right),
            new SelectorSide(0, ComponentChanges.Upper)
        };
        AdjustedSides(rotate);

        if (rotate < 0) rotate = 360 + rotate;

        FlipAxesForMove = (rotate > 45 && rotate < 135) || (rotate > 225 && rotate < 315);
        LessIsMoreX = (rotate > 225 && rotate < 315) || (rotate > 135 && rotate < 225);
        LessIsMoreY = (rotate > 45 && rotate < 135) || (rotate > 135 && rotate < 225);
    }

    public string GetCssClass(ComponentChanges changes) => Sides.First(s => s.Changes == changes).CssClass;

    public string GetAxis(ComponentChanges changes) => Sides.First(s => s.Changes == changes).Axis;

    public bool GetLessIsMore(ComponentChanges changes) => Sides.First(s => s.Changes == changes).LessIsMore;

   
    private void AdjustedSides(double rotate)
    {
        if (rotate < 0) rotate = 360 + rotate;

        Sides.ForEach(s =>
        {
            s.TotalAngle = s.OriginalAngle + rotate;
            if (s.TotalAngle > 360) s.TotalAngle = 360 - s.TotalAngle;
            if (s.TotalAngle == 360) s.TotalAngle = 0;           
        });

        foreach (var side in Sides.OrderByDescending(s => s.TotalAngle).Select((side, index) => new { Index = index, Side = side }))
        {
            switch (side.Index)
            {
                case 0://changing width - left
                    side.Side.CssClass = "null-left-resizer";
                    side.Side.Axis = "X";
                    switch (side.Side.Changes)
                    {
                        case ComponentChanges.Left:
                            //this is facing in the original direction
                            side.Side.LessIsMore = false;
                            break;
                        case ComponentChanges.Bottom:
                            
                            side.Side.LessIsMore = true;
                            break;
                        case ComponentChanges.Right:
                            side.Side.LessIsMore = true;
                            break;
                        case ComponentChanges.Upper:
                            side.Side.LessIsMore = false;
                            break;
                    }
                    break;
                case 1://changing length - bottom
                    side.Side.CssClass = "bottom-null-resizer";
                    side.Side.Axis = "Y";
                    switch (side.Side.Changes)
                    {
                        case ComponentChanges.Left:    
                            side.Side.LessIsMore = false;
                            break;
                        case ComponentChanges.Bottom:
                            //this is facing in the original direction
                            side.Side.LessIsMore = false;
                            break;
                        case ComponentChanges.Right:
                            side.Side.LessIsMore = false; //tested
                            break;
                        case ComponentChanges.Upper:
                            side.Side.LessIsMore = true;
                            break;
                    }
                    break;
                case 2://changing width - right
                    side.Side.CssClass = "null-right-resizer";
                    side.Side.Axis = "X";
                    switch (side.Side.Changes)
                    {
                        case ComponentChanges.Left:
                            side.Side.LessIsMore = false;
                            break;
                        case ComponentChanges.Bottom:
                            //this is facing in the original direction
                            side.Side.LessIsMore = false;
                            break;
                        case ComponentChanges.Right:
                            side.Side.LessIsMore = false;
                            break;
                        case ComponentChanges.Upper:
                            side.Side.LessIsMore = true;
                            break;
                    }
                    break;
                case 3:
                    side.Side.CssClass = "top-null-resizer";
                    side.Side.Axis = "Y";
                    switch (side.Side.Changes)
                    {
                        case ComponentChanges.Left:
                            side.Side.LessIsMore = false;
                            break;
                        case ComponentChanges.Bottom:
                            //this is facing in the original direction
                            side.Side.LessIsMore = false;
                            break;
                        case ComponentChanges.Right:
                            side.Side.LessIsMore = true;
                            break;
                        case ComponentChanges.Upper:
                            side.Side.LessIsMore = false; //tested
                            break;
                    }
                    break;
            }
        }

    }
}