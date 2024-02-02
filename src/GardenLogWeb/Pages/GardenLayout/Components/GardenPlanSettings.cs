namespace GardenLogWeb.Pages.GardenLayout.Components;


public record GardenPlanSettings(double GardenLength, double GardenWidth)
{
    public const int TickFootHeight = 48;
    public const int TickFootWidth = 48;
    public int TickInchHeight { get; set; } = 4;

    public int TickLength { get; set; } = 14;
    public int DivisionLength { get; set; } = 7;

    public double StartX { get; set; } = 0;
    public double StartY { get; set; } = 0;
    public double ViewBoxY { get; set; }
    public double ViewBoxX { get; set; }

    private int _margin = 20;
    private int _maxSvgHeight = 800;
    private int _maxSvgWidth = 1500;
    private double _zoom = 0;
    private double _viewBoxX;
    private double _viewBoxY;

    public double SvgHeight
    {
        get
        {
            if (GardenLength * TickFootHeight > _maxSvgHeight)
            {
                if (ViewBoxY == 0)
                {
                    ViewBoxY = _maxSvgHeight;
                    _viewBoxY = ViewBoxY;
                }
                return _maxSvgHeight;
            }
            else
            {
                if (ViewBoxY == 0)
                {
                    ViewBoxY = GardenLength * TickFootHeight + _margin;
                    _viewBoxY = ViewBoxY;
                }
                return GardenLength * TickFootHeight + _margin;
            }
        }
    }

    public double SvgWidth
    {
        get
        {
            if (GardenWidth * TickFootWidth > _maxSvgWidth)
            {
                if (ViewBoxX == 0)
                {
                    ViewBoxX = _maxSvgWidth;
                    _viewBoxX = ViewBoxX;
                }
                return _maxSvgWidth;
            }
            else
            {
                if (ViewBoxX == 0)
                {
                    ViewBoxX = GardenWidth * TickFootWidth + _margin;
                    _viewBoxX = ViewBoxX;
                }
                return GardenWidth * TickFootWidth + _margin;
            }
        }
    }

    public double Zoom
    {
        get { return _zoom; }
        set
        {
            _zoom = value;
            ViewBoxX = _viewBoxX + _zoom;
            ViewBoxY = _viewBoxY + _zoom;
           // Console.WriteLine($"ViewBoxX: {ViewBoxX} - _viewBoxX: {_viewBoxX} - _zoom: {_zoom}");
        }
    }

    public double GardenLayoutHeight
    {
        get
        {
            return GardenLength * TickFootHeight;
        }
    }

    public double GardenLayoutWidth
    {
        get
        {
            return GardenWidth * TickFootWidth;
        }
    }
    public void MoveDown()
    {
        //Console.WriteLine($"MoveDown - startY: {StartY}");
        if (StartY < GardenLength * TickFootHeight - 160)
            StartY += 160;
    }

    public void MoveUp()
    {
        //Console.WriteLine($"MoveUp - startY: {StartY}");
        if (StartY > 160)
            StartY -= 160;
        else if (StartY <= 160)
            StartY = 0;
    }

    public void MoveRight()
    {
       // Console.WriteLine($"MoveRight - _startX: {StartX}");
        if (StartX < GardenWidth * TickFootWidth)
            StartX += 100;
    }

    public void MoveLeft()
    {
        //Console.WriteLine($"MoveLeft - _startX: {StartX}");
        if (StartX > 100)
            StartX -= 100;
        else if (StartX <= 100)
            StartX = 0;
    }

}
