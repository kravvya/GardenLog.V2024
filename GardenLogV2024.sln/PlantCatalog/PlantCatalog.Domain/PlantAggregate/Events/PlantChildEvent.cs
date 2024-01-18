using GardenLog.SharedKernel;
using PlantCatalog.Domain.PlantAggregate.Events.Meta;

namespace PlantCatalog.Domain.PlantAggregate.Events
{
    public record PlantChildEvent : BaseDomainEvent
    {
        public PlantEventTriggerEnum Trigger { get; init; }
        public Meta.TriggerEntity TriggerEntity { get; init; }

        public PlantChildEvent(PlantEventTriggerEnum trigger, Meta.TriggerEntity entity)
        {
            Trigger = trigger;
            TriggerEntity = entity;
        }
    }
}
