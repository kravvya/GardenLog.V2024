using GardenLog.SharedKernel.Interfaces;
using MediatR;

namespace GardenLog.SharedKernel;

public abstract record BaseDomainEvent : IDomainEvent, INotification
{
    public string EventId { get; protected set; } = Guid.NewGuid().ToString();
    public DateTime EventDateUtc { get; protected set; } = DateTime.UtcNow;
    public TimeSpan EventDateUtcOffset { get; protected set; } = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
}


