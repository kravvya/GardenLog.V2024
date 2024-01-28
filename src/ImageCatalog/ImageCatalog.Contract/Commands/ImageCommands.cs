using FluentValidation;
using ImageCatalog.Contract.Base;

namespace ImageCatalog.Contract.Commands;

public record CreateImageCommand : ImageBase;


public record UpdateImageCommand
{
    public string ImageId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}


public class CreateImageCommandValidator : AbstractValidator<CreateImageCommand>
{
    public CreateImageCommandValidator()
    {
        RuleFor(command => command.ImageName).NotEmpty();
        RuleFor(command => command.Label).NotEmpty().Length(1, 100);
        RuleFor(command => command.RelatedEntityId).NotEmpty();
        RuleFor(command => command.RelatedEntityType).NotEmpty();
        RuleFor(command => command.FileName).NotEmpty();
        RuleFor(command => command.FileType).NotEmpty();
        RuleFor(command => command.RelatedEntities).NotEmpty();
    }
}

public class UpdateImageCommandValidator : AbstractValidator<UpdateImageCommand>
{
    public UpdateImageCommandValidator()
    {
        RuleFor(command => command.ImageId).NotEmpty().Length(3, 50);
        RuleFor(command => command.Label).NotEmpty().Length(3, 100);

    }
}

