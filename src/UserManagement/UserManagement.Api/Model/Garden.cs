namespace UserManagement.Api.Model;

public class Garden : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }=string.Empty;
    public string City { get; private set; } = string.Empty;
    public string StateCode { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public string UserProfileId { get; private set; } = string.Empty;
    public DateTime LastFrostDate { get; private set; }
    public DateTime FirstFrostDate { get; private set; }
    public DateTime WarmSoilDate { get; private set; }
    public double Length { get; private set; }
    public double Width { get; private set; }

    private readonly List<GardenBed> _gardenBeds = [];
    public IReadOnlyCollection<GardenBed> GardenBeds => _gardenBeds.AsReadOnly();

    public Garden() { }

    private Garden(string name, string city, string stateCode, decimal latitude, decimal longitude, string notes, string userProfileId, DateTime lastFrostDate, DateTime firstFrostDate, DateTime warmSoilDate, double length, double width, List<GardenBed> gardenBeds)
    {
        Name = name;
        City = city;
        StateCode = stateCode;
        Latitude = latitude;
        Longitude = longitude;
        Notes = notes;
        UserProfileId = userProfileId;
        _gardenBeds = gardenBeds;
        LastFrostDate = lastFrostDate;
        FirstFrostDate = firstFrostDate;
        WarmSoilDate = warmSoilDate;
        Length = length;
        Width = width;
    }

    public static Garden Create(
        string gardenName,
        string city,
        string stateCode,
        decimal latitude,
        decimal longitude,
        string notes,
        string userProfileId,
        DateTime lastFrostDate,
        DateTime firstFrostDate,
        DateTime warmSoilDate,
        double length,
        double width
    )
    {
        var garden = new Garden()
        {
            Id = Guid.NewGuid().ToString(),
            UserProfileId = userProfileId,
            Name = gardenName,
            City = city,
            StateCode = stateCode,
            Latitude = latitude,
            Longitude = longitude,
            Notes = notes,
            LastFrostDate= lastFrostDate,
            FirstFrostDate= firstFrostDate,
            WarmSoilDate   = warmSoilDate,
            Length = length,
            Width = width
        };

        garden.DomainEvents.Add(
                  new GardenEvent(garden, UserProfileEventTriggerEnum.GardenCreated, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.Garden, garden.Id)));

        return garden;
    }


    public void Update(
        string gardenName,
        string city,
        string stateCode,
        decimal latitude,
        decimal longitude,
        string notes,
        DateTime lastFrostDate,
        DateTime firstFrostDate,
        DateTime warmSoilDate,
        double length,
        double width
        )
    {
        this.Set<string>(() => this.Name, gardenName);
        this.Set<string>(() => this.City, city);
        this.Set<string>(() => this.StateCode, stateCode);
        this.Set<decimal>(() => this.Latitude, latitude);
        this.Set<decimal>(() => this.Longitude, longitude);
        this.Set<string>(() => this.Notes, notes);
        this.Set<DateTime>(() => this.LastFrostDate, lastFrostDate);
        this.Set<DateTime>(() => this.FirstFrostDate, firstFrostDate);
        this.Set<DateTime>(() => this.WarmSoilDate, warmSoilDate);
        this.Set<double>(() => this.Length, length);
        this.Set<double>(() => this.Width, width);
    }
    #region GardenBed
    public string AddGardenBed(CreateGardenBedCommand command)
    {
        command.GardenId = this.Id;
        var gardenBed = GardenBed.Create(command);

        this._gardenBeds.Add(gardenBed);

        this.DomainEvents.Add(
         new GardenEvent(this, UserProfileEventTriggerEnum.GardenBedCreated, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.GardenBed, gardenBed.Id)));

        return gardenBed.Id;
    }

    public void UpdateGardenBed(UpdateGardenBedCommand command)
    {
        this.GardenBeds.First(i => i.Id == command.GardenBedId).Update(command, AddChildDomainEvent);
    }

    public void DeleteGardenBed(string id)
    {
        AddChildDomainEvent(UserProfileEventTriggerEnum.GardenBedDeleted, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.GardenBed, id));

    }
    #endregion

    private void AddChildDomainEvent(UserProfileEventTriggerEnum trigger, UserManagment.Api.Model.Meta.TriggerEntity entity)
    {
        var newEvent = new GardenEvent(this, trigger, entity);

        if (!this.DomainEvents.Contains(newEvent))
            this.DomainEvents.Add(newEvent);
    }

    protected override void AddDomainEvent(string attributeName)
    {
        if (this.DomainEvents.Count == 0)
        {
            this.DomainEvents.Add(
                  new GardenEvent(this, UserProfileEventTriggerEnum.GardenUpdated, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.Garden, this.Id)));
        }
    }
}
