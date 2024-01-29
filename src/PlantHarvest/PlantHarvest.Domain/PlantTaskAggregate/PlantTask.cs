

using System.Threading.Tasks;

namespace PlantHarvest.Domain.WorkLogAggregate;

public class PlantTask : BaseEntity, IAggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public WorkLogReasonEnum Type { get; private set; }
    public DateTime CreatedDateTime { get; private set; }
    public DateTime TargetDateStart { get; private set; }
    public DateTime TargetDateEnd { get; private set; }
    public DateTime? CompletedDateTime { get; private set; }
    public string HarvestCycleId { get; private set; } = string.Empty;
    public string PlantHarvestCycleId { get; private set; } = string.Empty;
    public string PlantName { get; private set; } = string.Empty;
    public string PlantScheduleId { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public bool IsSystemGenerated { get; private set; }
    public string UserProfileId { get; private set; } = string.Empty;

    private PlantTask()
    {

    }

    private PlantTask(
        string title,
        WorkLogReasonEnum type,
        DateTime createdDateTime,
        DateTime targetDateStart,
        DateTime targetDateEnd,
        DateTime? completedDateTime,
        string harvestCycleId,
        string plantHarvestCycleId,
        string plantName,
        string plantScheduleId,
        string notes,
        bool isSystemGenerated,
        string userProfileId
        )
    {
        this.Title = title;
        this.Type = type;
        this.CreatedDateTime = createdDateTime;
        this.TargetDateStart = targetDateStart;
        this.TargetDateEnd = targetDateEnd;
        this.CompletedDateTime = completedDateTime;
        this.HarvestCycleId = harvestCycleId;
        this.PlantHarvestCycleId = plantHarvestCycleId;
        this.PlantName = plantName;
        this.PlantScheduleId = plantScheduleId;
        this.Notes = notes;
        this.IsSystemGenerated = isSystemGenerated;
        this.UserProfileId = userProfileId;
    }

    public static PlantTask Create(
        string title,
        WorkLogReasonEnum type,
        DateTime createdDateTime,
        DateTime targetDateStart,
        DateTime targetDateEnd,
        DateTime? completedDateTime,
        string harvestCycleId,
        string plantHarvestCycleId,
        string plantName,
        string plantScheduleId,
        string notes,
        bool isSystemGenerated,
        string userProfileId
        )
    {
       
        var task = new PlantTask()
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Type = type,
            CreatedDateTime = createdDateTime,
            TargetDateStart = targetDateStart,
            TargetDateEnd = targetDateEnd,
            CompletedDateTime = completedDateTime,
            HarvestCycleId = harvestCycleId,
            PlantHarvestCycleId = plantHarvestCycleId,
            PlantName = plantName,
            PlantScheduleId = plantScheduleId,
            Notes = notes,
            IsSystemGenerated = isSystemGenerated,
            UserProfileId = userProfileId
        };

        task.DomainEvents.Add(
            new PlantTaskEvent(task, PlantTaskEventTriggerEnum.PlantTaskCreated, new PlantTaskTriggerEntity(PlantTaskEntityTypeEnum.PlantTask, task.Id)));

        return task;
    }

    public void Update(
        DateTime targetDateStart,
        DateTime targetDateEnd,
        DateTime? completedDateTime,
        string notes     
        )
    {
        this.Set<DateTime>(() => this.TargetDateStart, targetDateStart);
        this.Set<DateTime>(() => this.TargetDateStart, targetDateStart);
        this.Set<DateTime?>(() => this.CompletedDateTime, completedDateTime);
        this.Set<string>(() => this.Notes, notes);

    }

    public void Delete()
    {
        this.DomainEvents.Add(
        new PlantTaskEvent(this, PlantTaskEventTriggerEnum.PlantTaskDeleted, new PlantTaskTriggerEntity(PlantTaskEntityTypeEnum.PlantTask, this.Id)));
    }

    protected override void AddDomainEvent(string attributeName)
    {
        PlantTaskEventTriggerEnum taskEvent = attributeName == "CompletedDate" && this.CompletedDateTime.HasValue ? 
                        PlantTaskEventTriggerEnum.PlantTaskCompleted : PlantTaskEventTriggerEnum.PlantTaskUpdated;
        this.DomainEvents.Add(
              new PlantTaskEvent(this, taskEvent, new PlantTaskTriggerEntity(PlantTaskEntityTypeEnum.PlantTask, this.Id)));
    }
}
