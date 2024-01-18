using GardenLog.SharedKernel;
using PlantCatalog.Domain.PlantAggregate.Events.Meta;

namespace PlantCatalog.Domain.PlantAggregate.Events
{
    public record PlantEvent : BaseDomainEvent
    {
        private string _plantId { get; init; } = string.Empty;
        public Plant? Plant { get; init; }
        public PlantEventTriggerEnum Trigger { get; init; }
        public Meta.TriggerEntity? TriggerEntity { get; init; }
        public string PlantId { get { return Plant!= null?Plant.Id: _plantId; } init { } }
        private PlantEvent() { }

        public PlantEvent(Plant plant, PlantEventTriggerEnum trigger, Meta.TriggerEntity entity)
        {
            Plant = plant;
            Trigger = trigger;
            TriggerEntity = entity;
        }

        public PlantEvent(string plantId, PlantEventTriggerEnum trigger, Meta.TriggerEntity entity)
        {
            _plantId = plantId;
            Trigger = trigger;
            TriggerEntity = entity;
        }


    }




}
