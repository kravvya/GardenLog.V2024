using PlantHarvest.Contract.Commands;

namespace PlantHarvest.Domain.HarvestAggregate;

public class GardenBedPlantHarvestCycle : BaseEntity, IEntity
{
    public string PlantHarvestCycleId { get; private set; } = string.Empty;
    public string PlantId { get; private set; } = string.Empty;
    public string PlantName { get; private set; } = string.Empty;
    public string PlantVarietyId { get; private set; } = string.Empty;
    public string PlantVarietyName { get; private set; } = string.Empty;
    public string GardenId { get; private set; } = string.Empty;
    public string GardenBedId { get; private set; } = string.Empty;
    public int NumberOfPlants { get; private set; } 
    public double PlantsPerFoot { get; private set; }

    public double X { get; private set; }
    public double Y { get; private set; }
    public double Length { get; private set; }
    public double Width { get; private set; }

    public double PatternWidth { get; private set; }
    public double PatternLength { get; private set; }

    protected GardenBedPlantHarvestCycle()
    {
    }


    public static GardenBedPlantHarvestCycle Create(CreateGardenBedPlantHarvestCycleCommand command)
    {
        return new GardenBedPlantHarvestCycle()
        {
            Id = Guid.NewGuid().ToString(),
            PlantHarvestCycleId = command.PlantHarvestCycleId,
            PlantId = command.PlantId,
            PlantName = command.PlantName,
            PlantVarietyId = command.PlantVarietyId??string.Empty,
            PlantVarietyName = command.PlantVarietyName ?? string.Empty,
            GardenId = command.GardenId,
            GardenBedId = command.GardenBedId,
            NumberOfPlants = command.NumberOfPlants,
            PlantsPerFoot = command.PlantsPerFoot,
            X = command.X,
            Y = command.Y,
            Length = command.Length,
            Width = command.Width,
            PatternLength = command.PatternLength,
            PatternWidth = command.PatternWidth
        };

    }


    public void Update(UpdateGardenBedPlantHarvestCycleCommand command, Action<HarvestEventTriggerEnum, TriggerEntity> addHarvestEvent)
    {
        this.Set<int>(() => this.NumberOfPlants, command.NumberOfPlants);
        this.Set<double>(() => this.PlantsPerFoot, command.PlantsPerFoot);
        this.Set<double>(() => this.X, command.X);
        this.Set<double>(() => this.Y, command.Y);
        this.Set<double>(() => this.Length, command.Length);
        this.Set<double>(() => this.Width, command.Width);
        this.Set<double>(() => this.PatternLength, command.PatternLength);
        this.Set<double>(() => this.PatternWidth, command.PatternWidth);

        if (this.DomainEvents != null && this.DomainEvents.Count > 0)
        {
            this.DomainEvents.Clear();
            addHarvestEvent(HarvestEventTriggerEnum.GardenBedPlantHarvestCycleUpdated, new TriggerEntity(EntityTypeEnum.GardenBedPlantHarvestCycle, this.Id));
        }
    }

    protected override void AddDomainEvent(string attributeName)
    {
        this.DomainEvents.Add(
            new HarvestChildEvent(HarvestEventTriggerEnum.GardenBedPlantHarvestCycleUpdated, new TriggerEntity(EntityTypeEnum.GardenBedPlantHarvestCycle, this.Id)));
    }
}
