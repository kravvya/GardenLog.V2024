using PlantHarvest.Contract.Commands;

namespace PlantHarvest.Domain.HarvestAggregate
{
    public class HarvestCycle : BaseEntity, IAggregateRoot
    {
        public string HarvestCycleName { get; private set; } = string.Empty;
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public string Notes { get; private set; } = string.Empty;
        public string UserProfileId { get; private set; } = string.Empty;
        public string GardenId { get; private set; } = string.Empty;

        private readonly List<PlantHarvestCycle> _plants = new();
        public IReadOnlyCollection<PlantHarvestCycle> Plants => _plants.AsReadOnly();

        private HarvestCycle()
        {

        }

        private HarvestCycle(
            string harvestCycleName,
            DateTime startDate,
            DateTime? endDate,
            string notes,
            string userProfileId,
            string gardenId
           )
        {
            this.UserProfileId = userProfileId;
            this.HarvestCycleName = harvestCycleName;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Notes = notes;
            this.GardenId = gardenId;
        }

        public static HarvestCycle Create(
            string userProfileId,
            string name,
            DateTime startDate,
            DateTime? endDate,
            string notes,
            string gardenId)
        {
            var harvest = new HarvestCycle()
            {
                Id = Guid.NewGuid().ToString(),
                UserProfileId = userProfileId,
                HarvestCycleName = name,
                StartDate = startDate,
                EndDate = endDate,
                Notes = notes,
                GardenId = gardenId
            };

            harvest.DomainEvents.Add(
            new HarvestEvent(harvest, HarvestEventTriggerEnum.HarvestCycleCreated, new TriggerEntity(EntityTypeEnum.HarvestCyce, harvest.Id)));

            return harvest;

        }


        public void Update(
            string name,
            DateTime startDate,
            DateTime? endDate,
            string notes,
            string gardenId
            )
        {
            this.Set<string>(() => this.HarvestCycleName, name);
            this.Set<DateTime>(() => this.StartDate, startDate);
            this.Set<DateTime?>(() => this.EndDate, endDate);
            this.Set<string>(() => this.Notes, notes);
            this.Set<string>(() => this.GardenId, gardenId);

            if(this.DomainEvents.FirstOrDefault(evt => evt is HarvestEvent && (((HarvestEvent)evt).Trigger) == HarvestEventTriggerEnum.HarvestCycleCompleted)!= null)
            {
                this.Plants.ToList().ForEach(p => p.CompletePlantHarvestCycle(this.EndDate, AddChildDomainEvent));
            }
         
        }

        public void Delete()
        {
            this.DomainEvents.Add(
         new HarvestEvent(this, HarvestEventTriggerEnum.HarvestCycleDeleted, new TriggerEntity(EntityTypeEnum.HarvestCyce, this.Id)));
        }

        #region Events
        protected override void AddDomainEvent(string attributeName)
        {
            switch (attributeName)
            {
                case "EndDate":
                    this.DomainEvents.Add(new HarvestEvent(this, HarvestEventTriggerEnum.HarvestCycleCompleted, new TriggerEntity(EntityTypeEnum.HarvestCyce, this.Id)));
                    break;
                default:
                    this.DomainEvents.Add(new HarvestEvent(this, HarvestEventTriggerEnum.HarvestCycleUpdated, new TriggerEntity(EntityTypeEnum.HarvestCyce, this.Id)));
                    break;
            }
        }

        private void AddChildDomainEvent(HarvestEventTriggerEnum trigger, TriggerEntity entity)
        {
            var newEvent = new HarvestEvent(this, trigger, entity);

            if (!this.DomainEvents.Contains(newEvent))
                this.DomainEvents.Add(newEvent);
        }
        #endregion

        #region Plants

        public void RehidratePlants(IReadOnlyCollection<PlantHarvestCycle> plants)
        {
            _plants.AddRange(plants);
        }

        public string AddPlantHarvestCycle(CreatePlantHarvestCycleCommand command)
        {
            command.HarvestCycleId = this.Id;
            var plant = PlantHarvestCycle.Create(command, AddChildDomainEvent);

            this._plants.Add(plant);

            this.DomainEvents.Add(
             new HarvestEvent(this, HarvestEventTriggerEnum.PlantAddedToHarvestCycle, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, plant.Id)));

            return plant.Id;
        }

        public void UpdatePlantHarvestCycle(UpdatePlantHarvestCycleCommand command)
        {
            this.Plants.First(i => i.Id == command.PlantHarvestCycleId).Update(command, AddChildDomainEvent);
        }

        public void DeletePlantHarvestCycle(string id)
        {
            AddChildDomainEvent(HarvestEventTriggerEnum.PlantHarvestCycleDeleted, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, id));

        }
        #endregion

        #region Plant Schedule
        public string AddPlantSchedule(CreatePlantScheduleCommand command)
        {

            var plant = _plants.First(p => p.Id == command.PlantHarvestCycleId);

            string scheduleId = plant.AddPlantSchedule(command);

            this.DomainEvents.Add(
             new HarvestEvent(this, HarvestEventTriggerEnum.PlantScheduleCreated, new TriggerEntity(EntityTypeEnum.PlantSchedule, scheduleId)));

            return scheduleId;
        }

        public void UpdatePlantSchedule(UpdatePlantScheduleCommand command)
        {
            var plant = _plants.First(p => p.Id == command.PlantHarvestCycleId);
            plant.UpdatePlantSchedule(command, AddChildDomainEvent);
        }

        public void DeletePlantSchedule(string plantHarvestCycleId, string plantScheduleId)
        {
            var plant = _plants.First(p => p.Id == plantHarvestCycleId);
            plant.DeletePlantSchedule(plantScheduleId);

            AddChildDomainEvent(HarvestEventTriggerEnum.PlantScheduleDeleted, new TriggerEntity(EntityTypeEnum.PlantSchedule, plantScheduleId));

        }

        public void DeleteAllSystemGeneratedSchedules(string plantHarvestCycleId)
        {
            var plant = _plants.First(p => p.Id == plantHarvestCycleId);
            plant.DeleteAllSystemGeneratedSchedules();
        }
        #endregion

        #region Garden Bed Layout
        public string AddGardenBedPlantHarvestCycle(CreateGardenBedPlantHarvestCycleCommand command)
        {

            var plant = _plants.First(p => p.Id == command.PlantHarvestCycleId);

            string gardenBedPlantId = plant.AddGardenBedPlantHarvestCycle(command);

            this.DomainEvents.Add(
             new HarvestEvent(this, HarvestEventTriggerEnum.GardenBedPlantHarvestCycleCreated, new TriggerEntity(EntityTypeEnum.GardenBedPlantHarvestCycle, gardenBedPlantId)));

            return gardenBedPlantId;
        }

        public void UpdateGardenBedPlantHarvestCycle(UpdateGardenBedPlantHarvestCycleCommand command)
        {
            var plant = _plants.First(p => p.Id == command.PlantHarvestCycleId);
            plant.UpdateGardenBedPlantHarvestCycle(command, AddChildDomainEvent);
        }

        public void DeleteGardenBedPlantHarvestCycle(string plantHarvestCycleId, string gardenBedPlantId)
        {
            var plant = _plants.First(p => p.Id == plantHarvestCycleId);
            plant.DeleteGardenBedPlantHarvestCycle(gardenBedPlantId);

            AddChildDomainEvent(HarvestEventTriggerEnum.GardenBedPlantHarvestCycleDeleted, new TriggerEntity(EntityTypeEnum.GardenBedPlantHarvestCycle, gardenBedPlantId));

        }
        #endregion
    }
}
