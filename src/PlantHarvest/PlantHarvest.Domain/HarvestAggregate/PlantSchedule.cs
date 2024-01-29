using PlantHarvest.Contract.Commands;

namespace PlantHarvest.Domain.HarvestAggregate;

public class PlantSchedule : BaseEntity, IEntity
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public WorkLogReasonEnum TaskType { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public bool IsSystemGenerated { get; private set; }

    private PlantSchedule()
    {
    }


    public static PlantSchedule Create(CreatePlantScheduleCommand command)
    {
        return new PlantSchedule()
        {
            Id = Guid.NewGuid().ToString(),
           StartDate= command.StartDate,
           EndDate= command.EndDate,
           TaskType = command.TaskType,
           Notes = command.Notes,
           IsSystemGenerated = command.IsSystemGenerated
        };

    }


    public void Update(UpdatePlantScheduleCommand command, Action<HarvestEventTriggerEnum, TriggerEntity> addHarvestEvent)
    {
        this.Set<DateTime>(() => this.StartDate, command.StartDate);
        this.Set<DateTime>(() => this.EndDate, command.EndDate);
        this.Set<WorkLogReasonEnum>(() => this.TaskType, command.TaskType);
        this.Set<string?>(() => this.Notes, command.Notes);
        this.Set<bool>(() => this.IsSystemGenerated, command.IsSystemGenerated);
       
        if (this.DomainEvents != null && this.DomainEvents.Count > 0)
        {
            this.DomainEvents.Clear();
            addHarvestEvent(HarvestEventTriggerEnum.PlantScheduleUpdated, new TriggerEntity(EntityTypeEnum.PlantSchedule, this.Id));
        }
    }

    protected override void AddDomainEvent(string attributeName)
    {
        this.DomainEvents.Add(
            new HarvestChildEvent(HarvestEventTriggerEnum.PlantScheduleUpdated, new TriggerEntity(EntityTypeEnum.PlantSchedule, this.Id)));
    }
}
