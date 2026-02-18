
using GardenLog.SharedKernel.Enum;
using PlantHarvest.Contract.Query;

namespace PlantHarvest.Domain.WorkLogAggregate;

public interface IWorkLogRepository : IRepository<WorkLog>
{
    Task<IReadOnlyCollection<WorkLogViewModel>> GetWorkLogsByEntity(RelatedEntityTypEnum entityType, string entityId, string userProfileId);
    Task<IReadOnlyCollection<WorkLogViewModel>> SearchWorkLogsForUser(WorkLogSearch search, string userProfileId);
}
