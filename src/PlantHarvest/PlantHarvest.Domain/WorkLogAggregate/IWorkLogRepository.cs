
using GardenLog.SharedKernel.Enum;

namespace PlantHarvest.Domain.WorkLogAggregate;

public interface IWorkLogRepository : IRepository<WorkLog>
{
    Task<IReadOnlyCollection<WorkLogViewModel>> GetWorkLogsByEntity(RelatedEntityTypEnum entityType, string entityId, string userProfileId);
}
