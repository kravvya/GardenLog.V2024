using MongoDB.Driver;

namespace UserManagement.Api.Model;

public class Weatherstation : BaseEntity, IEntity
{
    public string ForecastOffice { get; private set; } = string.Empty;
    public double GridX { get; private set; }
    public double GridY { get; private set; }
    public string Timezone { get; private set; } = string.Empty;

    public Weatherstation() { }

    public static Weatherstation Create(string forecastOffice, double gridX, double gridY, string timezone)
    {
        var weatherStation = new Weatherstation()
        {
            Id = Guid.NewGuid().ToString(),
            ForecastOffice = forecastOffice,
            GridX = gridX,
            GridY = gridY,
            Timezone = timezone
        };
        
        return weatherStation;
    }

    public void Update(string forecastOffice, double gridX, double gridY, string timezone, Action<UserProfileEventTriggerEnum, TriggerEntity> addGardenEvent)
    {
        this.Set<string>(() => this.ForecastOffice, forecastOffice);
        this.Set<double>(() => this.GridX, gridX);
        this.Set<double>(() => this.GridY, gridY);
        this.Set<string>(() => this.Timezone, timezone);
        
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
                  new GardenChildEvent(UserProfileEventTriggerEnum.WeatherstationUpdated, new TriggerEntity(EntityTypeEnum.WeatherStation, this.Id)));
        }
    }
}
