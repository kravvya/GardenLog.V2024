using FluentValidation;
using GardenLog.SharedKernel.Enum;

namespace ImageCatalog.Contract.Queries;

public record GetImagesByRelatedEntity(RelatedEntityTypEnum RelatedEntityType, string? RelatedEntityId, bool FilterUserOnly);

public record GetImagesByRelatedEntities
{
    public List<GetImagesByRelatedEntity>? Requests { get; set; }
}

public class GetImagesByRelatedEntityValidator : AbstractValidator<GetImagesByRelatedEntity>
{
    public GetImagesByRelatedEntityValidator()
    {
        RuleFor(command => command.RelatedEntityType).NotEmpty();
    }
}