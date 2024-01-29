using GardenLog.SharedKernel.Enum;
using PlantHarvest.Domain.WorkLogAggregate.Events;

namespace PlantHarvest.Domain.WorkLogAggregate;

public class WorkLog : BaseEntity, IAggregateRoot
{
    public string Log { get; private set; } = string.Empty;
    public DateTime EnteredDateTime { get; private set; }
    public DateTime EventDateTime { get; private set; }
    public WorkLogReasonEnum Reason { get; private set; }
    public string UserProfileId { get; private set; } = string.Empty;
    public IList<RelatedEntity>? RelatedEntities { get; private set; }

    private WorkLog()
    {

    }

    public WorkLog(
        string log,
        DateTime enteredDateTime,
        DateTime eventDateTime,
        WorkLogReasonEnum reason,
        string userProfileId,
        IList<RelatedEntity> relatedEntities)
    {
        this.Log = log;
        this.EnteredDateTime = enteredDateTime;
        this.EventDateTime = eventDateTime;
        this.Reason = reason;
        this.UserProfileId = userProfileId;
        this.RelatedEntities = relatedEntities;
    }

    public static WorkLog Create(
        string log,
        DateTime eventDateTime,
        WorkLogReasonEnum reason,
        string userProfileId,
        IList<RelatedEntity> relatedEntities
        )
    {
        DateTime timestamp = DateTime.Now;

        var work = new WorkLog()
        {
            Id = Guid.NewGuid().ToString(),
            Log = log,
            RelatedEntities = relatedEntities,
            EnteredDateTime = timestamp,
            EventDateTime = eventDateTime,
            Reason = reason,
            UserProfileId = userProfileId
        };

        work.DomainEvents.Add(
            new WorkLogEvent(work, WorkLogEventTriggerEnum.WorkLogCreated, new RelatedEntity(RelatedEntityTypEnum.WorkLog, work.Id, string.Empty)));

        return work;
    }

    public void Update(
        string log,
        DateTime eventDateTime,
        WorkLogReasonEnum reason)
    {
        this.Set<string>(() => this.Log, log);
        this.Set<DateTime>(() => this.EventDateTime, eventDateTime);
        this.Set<WorkLogReasonEnum>(() => this.Reason, reason);

    }

    public void Delete()
    {
        this.DomainEvents.Add(
            new WorkLogEvent(this, WorkLogEventTriggerEnum.WorkLogDeleted, new RelatedEntity(RelatedEntityTypEnum.WorkLog, this.Id, string.Empty)));
    }

    protected override void AddDomainEvent(string attributeName)
    {
        this.DomainEvents.Add(
              new WorkLogEvent(this, WorkLogEventTriggerEnum.WorkLogUpdated, new RelatedEntity(RelatedEntityTypEnum.WorkLog, this.Id, string.Empty)));
    }
}
