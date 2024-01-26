namespace UserManagement.Api.Model;

public class GardenBed : BaseEntity, IEntity
{
    public string Name { get; private set; } = string.Empty;
    public int? RowNumber { get; private set; }
    public double Length { get; private set; }
    public double Width { get; private set; }
    public double X { get; private set; }
    public double Y { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public GardenBedTypeEnum Type { get; private set; }
    public double Rotate { get; set; }

    public GardenBed() { }

    public static GardenBed Create(CreateGardenBedCommand command)
    {
        var gardenBed = new GardenBed()
        {
            Id = Guid.NewGuid().ToString(),
            Name = command.Name,
            RowNumber = command.RowNumber,
            Length = command.Length,
            Width = command.Width,
            X = command.X,
            Y = command.Y,
            Notes = command.Notes,
            Type = command.Type,
            Rotate = command.Rotate,
        };

        gardenBed.DomainEvents.Add(
                  new GardenChildEvent(UserProfileEventTriggerEnum.GardenBedCreated, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.GardenBed, gardenBed.Id)));

        return gardenBed;
    }


    public void Update(UpdateGardenBedCommand command, Action<UserProfileEventTriggerEnum, UserManagment.Api.Model.Meta.TriggerEntity> addGardenEvent)
    {
        this.Set<string>(() => this.Name, command.Name);
        this.Set<int?>(() => this.RowNumber, command.RowNumber);
        this.Set<double>(() => this.Length, command.Length);
        this.Set<double>(() => this.Width, command.Width);
        this.Set<double>(() => this.X, command.X);
        this.Set<double>(() => this.Y, command.Y);
        this.Set<string>(() => this.Notes, command.Notes);
        this.Set<double>(() => this.Rotate, command.Rotate);

        if (this.DomainEvents != null && this.DomainEvents.Count > 0)
        {
            this.DomainEvents.Clear();
            addGardenEvent(UserProfileEventTriggerEnum.GardenBedUpdated, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.GardenBed, this.Id));
        }
    }

    protected override void AddDomainEvent(string attributeName)
    {
        if (this.DomainEvents.Count == 0)
        {
            this.DomainEvents.Add(
                  new GardenChildEvent(UserProfileEventTriggerEnum.GardenBedUpdated, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.GardenBed, this.Id)));
        }
    }
}
