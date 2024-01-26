namespace UserManagement.Contract.Validators;

public class EmailMessageValidator<T> : AbstractValidator<T>
    where T : EmailMessageBase
{
    public EmailMessageValidator()
    {
        RuleFor(command => command.Name).NotEmpty().Length(3, 50);
        RuleFor(command => command.EmailAddress).NotEmpty().EmailAddress();
        RuleFor(command => command.Subject).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Message).NotEmpty().MaximumLength(1000);
    }
}
