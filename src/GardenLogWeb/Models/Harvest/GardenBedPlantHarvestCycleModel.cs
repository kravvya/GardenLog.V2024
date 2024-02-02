using GardenLogWeb.Pages.GardenLayout.Components;
using GardenLogWeb.Pages.HarvestGardenLayout.Components;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace GardenLogWeb.Models.Harvest
{
    public record GardenBedPlantHarvestCycleModel : GardenBedPlantHarvestCycleViewModel
    {
        public string ImageFileName { get; set; } = string.Empty;
        public string ImageLabel { get; set; } = string.Empty;

        //public void SetLengthAndWidth(double bedLength, double bedWidth)
        //{

        //    if (PlantsPerFoot > 1)
        //    {
        //        PatternWidth = 1;
        //        PatternLength = 1;
        //        // var plantsPerRow = (bedWidth / 12) * PlantsPerFoot;

        //        // Length = Math.Ceiling(NumberOfPlants / plantsPerRow);
        //        // Width = (bedWidth / 12);
        //    }
        //    else if (PlantsPerFoot == 1)
        //    {
        //        PatternWidth = 1;
        //        PatternLength = 1;
        //        // Length = Math.Ceiling(NumberOfPlants / (bedWidth / 12));
        //        // Width = Math.Round((bedWidth / 12), 0, MidpointRounding.ToZero);
        //    }
        //    else if (PlantsPerFoot < 1)
        //    {
        //        PatternWidth = Math.Ceiling(1 / PlantsPerFoot);
        //        PatternLength = PatternWidth;

        //        // Width = Math.Round((bedWidth / 12) / PatternWidth,0,MidpointRounding.ToZero);
        //        // Length =Math.Ceiling(NumberOfPlants / Width);

        //    }
        //    Length = PatternLength;
        //    Width = PatternWidth;
        //    Console.WriteLine($"PlantsPerFoot - {PlantsPerFoot} Length - {Length} Width - {Width}");
        //}

        /*
            * Step1 : Figure out how many plants in one row: 
            *  PlantsPerFoot * NumberofFeetInPattern * NumberOfPatterns in 1 row
            * Step2 : Figure out how many rows required for the number of plants
            *   numberOfPlants/number of plants in a row
            * Step3 : How many rows can fit into a garden bed
            *    bedLength/pattern length
            * Step4 : If number of rows in the garden bed <= number of requred beds set Length and Width
            *     Width = Max number of pattern in a row
            *     Length = numner of rows required 
            */

        /*
         * Units of Meassure:
         * bedLength - in
         * bedWidth - in
         * PatternWidth - ft
         * PatternLength - ft
         * Length - ft
         * Width - ft
         * 
         */
        public void SetLengthAndWidth(double bedLength, double bedWidth)
        {         

            if (PlantsPerFoot > 1)
            {
                PatternWidth = 1;
                PatternLength = 1;
            }
            else if (PlantsPerFoot == 1)
            {
                PatternWidth = 1;
                PatternLength = 1;
            }
            else if (PlantsPerFoot < 1)
            {
                PatternWidth = Math.Ceiling(1 / PlantsPerFoot);
                PatternLength = PatternWidth;
            }

            //Step1 : Figure out how many plants in one row: 
            //PlantsPerFoot * NumberofFeetInPattern * NumberOfPatterns in 1 row
            var numberOfPatternsInRow = Math.Floor((bedWidth / 12) / PatternWidth);
            var plantsPerRow = PlantsPerFoot * PatternWidth * numberOfPatternsInRow;
            if (numberOfPatternsInRow == 0)
            {
                //numberOfPatternsInRow = 1;
                plantsPerRow = 1;
                Width = bedWidth / 12;
            }
            else
            {
                Width = numberOfPatternsInRow * PatternWidth;
            }
          
            //Step2 : Figure out how many rows required for the number of plants
            // numberOfPlants/number of plants in a row
            var numberOfRowsRequired = Math.Ceiling(NumberOfPlants / plantsPerRow);

            //Step3 : How many rows can fit into a garden bed
            //bedLength / pattern length
            var numbeOfRowsAvailable = (bedLength / 12) / PatternLength;

            //Step4 : If number of rows in the garden bed <= number of requred beds set Length and Width
            //     Width = Max number of pattern in a row
            //     Length = numner of rows required 

            Length = numberOfRowsRequired <= numbeOfRowsAvailable ? numberOfRowsRequired* PatternLength : numbeOfRowsAvailable* PatternLength;       
           
        }

        public int NumberOfPlantsPerBed(double bedLength, double bedWidth)
        {
            var patternWidth =0;
            var patternLength = 0;

            if (PlantsPerFoot > 1)
            {
                 patternWidth = 1;
                 patternLength = 1;
            }
            else if (PlantsPerFoot == 1)
            {
                patternWidth = 1;
                patternLength = 1;
            }
            else if (PlantsPerFoot < 1)
            {
                patternWidth = Convert.ToInt32(Math.Ceiling(1 / PlantsPerFoot));
                patternLength = patternWidth;
            }
            var numberOfPatternsInRow = Math.Ceiling((bedWidth / 12) / patternWidth);
            var numbeOfRows = (bedLength / 12) / patternLength;
            return Convert.ToInt32(numberOfPatternsInRow * numbeOfRows * PlantsPerFoot);
        }

        public double GetHeightInPixels()
        {
            return this.Length * GardenPlanSettings.TickFootHeight;
        }

        public double GetWidthInPixels()
        {
            return this.Width * GardenPlanSettings.TickFootWidth;
        }

        public double GetPatternHeightInPixels()
        {
            return this.PatternLength * GardenPlanSettings.TickFootHeight;
        }

        public double GetPatternWidthInPixels()
        {
            return this.PatternWidth * GardenPlanSettings.TickFootWidth;
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

        public void IncreaseLengthByPatternUnits(double patternUnits)
        {
            Length += patternUnits * PatternLength;
            if (Length <= 1) Length = 1;

            CalcualteNumberOfPlants();
        }

        public void IncreaseWidthByPatternUnits(double patternUnits)
        {
            Width += patternUnits * PatternWidth;
            if (Width <= 1) Width = 1;

            CalcualteNumberOfPlants();
        }

        private void CalcualteNumberOfPlants()
        {
            var plantsInRow = Width / PatternWidth;
            if (plantsInRow < 1) { plantsInRow = 1; }

            var plantsPerFoot = PlantsPerFoot > 1 ? PlantsPerFoot : 1;
            NumberOfPlants = Convert.ToInt32(plantsPerFoot * Length / PatternLength * plantsInRow);
        }

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
    }
}
