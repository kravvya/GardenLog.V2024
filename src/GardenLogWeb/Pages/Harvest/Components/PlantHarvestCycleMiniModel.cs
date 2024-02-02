using System.ComponentModel;

namespace GardenLogWeb.Pages.Harvest.Components
{
    public record PlantHarvestCycleMiniModel(string HarvestCycleId, string PlantHarvestCycleId)
    {
        public int? NumberOfSeeds { get; set; }

        public string? SeedVendorId { get; set; }
        public string? SeedVendorName { get; set; }

        public DateTime? SeedingDate { get; set; } = DateTime.Now;

        public DateTime? GerminationDate { get; set; } = DateTime.Now;
        public decimal? GerminationRate { get; set; }

        public int? NumberOfTransplants { get; set; }
        public DateTime? TransplantDate { get; set; } = DateTime.Now;

        public bool IsLastHarvest { get; set; }
        public DateTime? HarvestDate { get; set; } = DateTime.Now;

        public decimal? WeightInPounds { get; set; }

        public int? Items { get; set; }
    }
}
