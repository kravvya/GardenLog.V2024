using GardenLog.SharedKernel;
using MediatR;

namespace PlantHarvest.Api.Extensions;

static class MediatorExtension
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, BaseEntity entity)
    {
        var domainEvents = entity.DomainEvents.ToList();

        var tasks = domainEvents.Select(async (domainEvent) =>
                     {
                         await mediator.Publish(domainEvent);
                     });

        await Task.WhenAll(tasks);

        entity.DomainEvents.Clear();
    }
}
