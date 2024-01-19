using GardenLog.SharedKernel.Enum;

namespace GardenLog.SharedKernel;

public record RelatedEntity(RelatedEntityTypEnum EntityType, string EntityId, string EntityName)
{
}
